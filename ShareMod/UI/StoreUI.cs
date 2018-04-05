using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GUILayout;

namespace ShareMod.UI
{
    internal class StoreUI : UIScreen
    {
        private enum WorldState
        {
            None,
            Downloading,
            Existing
        }

        private enum State
        {
            Idle,
            Loading,
            Error
        }

        private class WorldItem
        {
            public WorldModel World { get; }
            public UserModel Author { get; set; }
            public WorldState State { get; set; }
            public string TimeString { get; set; }

            public WorldItem() { }
            public WorldItem(WorldModel world, UserModel author, int? currentTime = null)
            {
                currentTime = currentTime ?? (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                this.World = world;
                this.Author = author;
                UpdateTime(currentTime.Value);

                if (Directory.Exists(SavesPath + this.World.Title))
                    this.State = WorldState.Existing;
                else
                    this.State = WorldState.None;
            }

            public void UpdateTime(int time)
            {
                this.TimeString = GetTimeString(this.World.CreatedAt, time);
            }

            private string GetTimeString(int timestamp, int currentTime)
            {
                int seconds = currentTime - timestamp;
                float minutes = seconds / 60f;
                float hours = minutes / 60;
                float days = hours / 24;
                float weeks = days / 7;

                float value;
                string unit = "";

                if ((int)weeks > 0)
                {
                    value = weeks;
                    unit = "weeek";
                }
                else if ((int)days > 0)
                {
                    value = days;
                    unit = "day";
                }
                else if ((int)hours > 0)
                {
                    value = hours;
                    unit = "hour";
                }
                else if ((int)minutes > 0)
                {
                    value = minutes;
                    unit = "minute";
                }
                else
                {
                    value = seconds;
                    unit = "second";
                }

                if ((int)value != 1)
                {
                    unit += "s";
                }

                return $"{(int)value} {unit} ago";
            }
        }

        public override bool RequireMainMenu => true;

        private Rect WorldsWindowRect = new Rect(0, 0, 500, 500);
        private Vector2 ScrollPos = new Vector2();
        private WorldModel Selected = null;
        private WorldsResponseModel LastResponse = null;
        private Dictionary<WorldModel, WorldItem> WorldStates = new Dictionary<WorldModel, WorldItem>();
        private GUIStyle EntryStyle, EntryHoverStyle, WorldInfoStyle, WorldIDStyle;
        private int CurrentPage = 0;
        private float LastTimeUpdate = 0;
        private Dictionary<int, WorldsResponseModel> PageCache = new Dictionary<int, WorldsResponseModel>();
        private State CurrentState = State.Idle;
        private IList<ManageWorldUI> Managing = new List<ManageWorldUI>();

        private WorldModel[] Worlds => LastResponse?.Items;
        
        private static string SavesPath => Application.persistentDataPath + "/saves/";


        public StoreUI(Remote remote) : base(remote)
        {
            WorldsWindowRect.x = Screen.width / 2 - WorldsWindowRect.width / 2;
            WorldsWindowRect.y = Screen.height / 2 - WorldsWindowRect.height / 2;
        }

        public override void Init()
        {
            Refresh();

            EntryStyle = new GUIStyle
            {
                hover = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(1, 1, 1, 0.15f))
                }
            };

            EntryHoverStyle = new GUIStyle(EntryStyle)
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(0, 0, 0, 0.3f))
                },
                hover = new GUIStyleState()
            };

            WorldInfoStyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    textColor = GUI.contentColor
                },
                padding = new RectOffset(5, 5, 1, 1)
            };

            WorldIDStyle = new GUIStyle(WorldInfoStyle)
            {
                alignment = TextAnchor.MiddleRight
            };

            base.Init();

            IGConsole.Log("Init");
        }

        public override void Draw()
        {
            if (!IsInitialized || !Remote.User.IsLoggedIn || !RunMainMenu.Instance.MainMenuCanvas.enabled)
                return;

            //Update world "created at" times every second
            if (Time.time - LastTimeUpdate > 1)
            {
                LastTimeUpdate = Time.time;
                UpdateWorldTimes();
            }

            if (GUI.Button(new Rect(2, 67, 150, 25), "Browse market") || (Visible && Input.GetKeyDown(KeyCode.Escape)))
            {
                ShareMod.PlayButtonSound();

                Visible = !Visible;
                SetMainMenuPlaying(!Visible);
            }

            if (Visible)
            {
                GUI.Window(1237, WorldsWindowRect, o => { }, "");
                WorldsWindowRect = GUI.Window(1235, WorldsWindowRect, DrawWindow, "World market");
                GUI.BringWindowToFront(1235);
            }
        }

        private void DrawWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, WorldsWindowRect.width, 25));

            BeginVertical();
            {
                ScrollPos = BeginScrollView(ScrollPos, false, true, Width(WorldsWindowRect.width - 13), MinHeight(WorldsWindowRect.height - 60));
                {
                    if (LastResponse != null)
                    {
                        foreach (var item in WorldStates)
                        {
                            DrawWorldEntry(item.Value);
                        }
                    }
                }
                EndScrollView();

                BeginHorizontal();
                {
                    if (CurrentState == State.Loading)
                    {
                        Label("<size=20>Loading...</size>");
                    }
                    else if (CurrentState == State.Error)
                    {
                        Label("<size=20>Couldn't contact server</size>");
                    }
                    else
                    {
                        Label($"<size=20>Page {LastResponse.Page + 1}</size>");
                    }

                    if (CurrentState == State.Idle)
                        DrawPageControls();
                }
                EndHorizontal();
            }
            EndVertical();
        }

        private void DrawPageControls(bool enabled = true)
        {
            bool downloading = WorldStates.Values.Any(o => o.State == WorldState.Downloading);
            if (downloading)
            {
                enabled = false;
            }

            FlexibleSpace();

            BeginVertical();
            {
                FlexibleSpace();

                GUI.enabled = !downloading;
                if (Button("Refresh", Height(30), Width(95)))
                {
                    ShareMod.PlayButtonSound();

                    Refresh(clearCache: true);
                }
                GUI.enabled = true;

                FlexibleSpace();
            }
            EndVertical();

            Space(8);

            BeginVertical();
            {
                FlexibleSpace();

                GUI.enabled = downloading ? false : !enabled ? false : (LastResponse?.Page ?? 0) != 0;
                if (Button("Previous page", Height(30), Width(95)))
                {
                    ShareMod.PlayButtonSound();

                    CurrentPage--;
                    Refresh();
                }

                FlexibleSpace();
            }
            EndVertical();

            BeginVertical();
            {
                FlexibleSpace();

                GUI.enabled = downloading ? false : !enabled ? false : (!LastResponse?.LastPage ?? false);
                if (Button("Next page", Height(30), Width(95)))
                {
                    ShareMod.PlayButtonSound();

                    CurrentPage++;
                    Refresh();
                }
                GUI.enabled = enabled;

                FlexibleSpace();
            }
            EndVertical();
        }

        private void DrawWorldEntry(WorldItem world)
        {
            string authorName = world.Author.Username;
            
            BeginHorizontal(world.World == Selected ? EntryHoverStyle : EntryStyle, Height(68));
            {
                Box("", Width(60), Height(60));

                BeginVertical();
                {
                    Label($"<b><size=16>{world.World.Title}</size></b>");

                    Label("by " + authorName, WorldInfoStyle);
                    Label($"{world.World.Downloads} downloads", WorldInfoStyle);
                }
                EndVertical();
                
                string btnText =
                    world.State == WorldState.Downloading ? "Downloading..." :
                    world.State == WorldState.Existing ? "Play" : "Download";
                
                GUI.enabled = world.State != WorldState.Downloading;
                FlexibleSpace();
                BeginVertical();
                {
                    FlexibleSpace();

                    BeginHorizontal();
                    {
                        FlexibleSpace();

                        //--Manage button--
                        if (world.Author.ID == Remote.User.CurrentUser.ID && Button("Manage", Height(30)))
                        {
                            ShareMod.PlayButtonSound();

                            if (!Managing.Any(o => o.World.ID == world.World.ID))
                            {
                                ShowManageWorld(world.World);
                            }
                        }
                        //------------
                        
                        //--Download button--
                        if (Button(btnText, Height(30), Width(95)))
                        {
                            ShareMod.PlayButtonSound();
                            Selected = world.World;

                            if (world.State == WorldState.Existing)
                            {
                                Hide();
                                LoadWorld(world.World);
                            }
                            else
                            {
                                DownloadWorld(world);
                            }
                        }
                        //------------
                    }
                    EndHorizontal();

                    Space(5);

                    GUI.enabled = false;

                    string time = world.TimeString;
                    
                    Label(time, WorldIDStyle);
                    FlexibleSpace();

                    GUI.enabled = true;
                }
                EndVertical();
            }
            EndHorizontal();

            Rect horRect = GUILayoutUtility.GetLastRect();

            if (Event.current.isMouse && horRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                Selected = world.World;
            }
        }

        private void ShowManageWorld(WorldModel world)
        {
            var window = new ManageWorldUI(Remote, world);
            window.Init();
            window.Visible = true;
            window.Closed += (a, b) =>
            {
                Managing.Remove(window);
                ShareMod.Screens.Remove(window);
            };
            window.Refresh += (a, b) =>
            {
                PageCache.Remove(CurrentPage);
                Refresh();
            };

            ShareMod.Screens.Add(window);
            Managing.Add(window);
        }

        private void LoadWorld(WorldModel world)
        {
            SaveManager.SaveName = world.Title;
            GameplaySaving.LoadLegacySave = false;
            SceneManager.LoadScene("gameplay");
            EverythingHider.HideEverything();
        }

        private void UpdateWorldTimes()
        {
            int time = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            foreach (var item in WorldStates)
            {
                item.Value.UpdateTime(time);
            }
        }

        public void Refresh(bool resetPage = false, bool clearCache = false)
        {
            CurrentState = State.Loading;

            if (resetPage)
                CurrentPage = 0;

            if (clearCache)
                PageCache.Clear();

            LastResponse = null;

            WorldStates.Clear();

            new Thread(() =>
            {
                if (!PageCache.ContainsKey(CurrentPage))
                {
                    var resp = Remote.GetWorlds(CurrentPage);
                    
                    if (resp != null)
                        PageCache[CurrentPage] = resp;
                }

                if (!PageCache.TryGetValue(CurrentPage, out LastResponse) || LastResponse?.Status != "ok")
                {
                    CurrentState = State.Error;

                    return;
                }
                
                CurrentState = State.Idle;
                ScrollPos = Vector2.zero;

                foreach (var item in Worlds)
                {
                    var state = new WorldItem(item, Remote.User.GetByID(item.AuthorID));

                    WorldStates.Add(item, state);

                    if (Directory.Exists(SavesPath + item.Title))
                    {
                        state.State = WorldState.Existing;
                    }
                    else
                    {
                        state.State = WorldState.None;
                    }
                }
            }).Start();
        }

        private void DownloadWorld(WorldItem world)
        {
            world.State = WorldState.Downloading;

            Remote.DownloadWorld(world.World.ID, o =>
            {
                SaveWorld(world.World, o);
                world.State = WorldState.Existing;

                Console.WriteLine(o.Length);
            });
        }

        private void SaveWorld(WorldModel world, byte[] data)
        {
            MemoryStream inStream = new MemoryStream(data);

            Zipper.ExtractZipFile(inStream, SavesPath + world.Title);
        }
    }
}
