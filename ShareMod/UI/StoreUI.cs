using ICSharpCode.SharpZipLib.Zip;
using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
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

        public override bool RequireMainMenu => true;

        private Rect WorldsWindowRect = new Rect(0, 0, 500, 500);
        private Vector2 ScrollPos = new Vector2();
        private WorldModel Selected = null;
        private WorldsResponseModel LastResponse = null;
        private IDictionary<WorldModel, WorldState> WorldStates = new Dictionary<WorldModel, WorldState>();
        private IDictionary<WorldModel, string> WorldTimes = new Dictionary<WorldModel, string>();
        private GUIStyle EntryStyle, EntryHoverStyle, WorldInfoStyle, WorldIDStyle;
        private int CurrentPage = 0;
        private float LastTimeUpdate = 0;
        private IDictionary<int, WorldsResponseModel> PageCache = new Dictionary<int, WorldsResponseModel>();
        private State CurrentState = State.Idle;

        private WorldModel[] Worlds => LastResponse?.Items;


        private string SavesPath => Application.persistentDataPath + "/saves/";

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
        }

        public override void Draw()
        {
            if (!IsInitialized || !Remote.User.IsLoggedIn)
                return;

            //Update world "created at" times every second
            if (Time.time - LastTimeUpdate > 1)
            {
                LastTimeUpdate = Time.time;
                UpdateWorldTimes();
            }

            if (GUI.Button(new Rect(2, 67, 150, 25), "Browse market") || (Visible && Input.GetKeyDown(KeyCode.Escape)))
            {
                Visible = !Visible;
                ShareMod.PlayButtonSound();

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
                        foreach (var item in Worlds)
                        {
                            DrawWorldEntry(item);
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
            FlexibleSpace();

            BeginVertical();
            {
                FlexibleSpace();

                if (Button("Refresh", Height(30), Width(95)))
                {
                    ShareMod.PlayButtonSound();

                    Refresh(clearCache: true);
                }

                FlexibleSpace();
            }
            EndVertical();

            Space(8);

            BeginVertical();
            {
                FlexibleSpace();

                GUI.enabled = !enabled ? false : (LastResponse?.Page ?? 0) != 0;
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

                GUI.enabled = !enabled ? false : (!LastResponse?.LastPage ?? false);
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

        private void DrawWorldEntry(WorldModel world)
        {
            string authorName = Remote.User.GetByID(world.AuthorID)?.Username;

            BeginHorizontal(world == Selected ? EntryHoverStyle : EntryStyle, Height(68));
            {
                Box("", Width(60), Height(60));

                BeginVertical();
                {
                    Label($"<size=16>{world.Title}</size>");

                    Label("by " + authorName, WorldInfoStyle);
                    Label($"{world.Downloads} downloads", WorldInfoStyle);
                }
                EndVertical();


                //Try to get the world state from WorldStates. If it doesn't exist, add it.
                if (!WorldStates.TryGetValue(world, out WorldState state))
                    state = WorldStates[world] = WorldState.None;

                string btnText =
                    state == WorldState.Downloading ? "Downloading..." :
                    state == WorldState.Existing ? "Installed" : "Download";
                
                GUI.enabled = state != WorldState.Downloading && state != WorldState.Existing;
                FlexibleSpace();
                BeginVertical();
                {
                    //--Download button--
                    FlexibleSpace();

                    BeginHorizontal();
                    {
                        FlexibleSpace();
                        if (Button(btnText, Height(30), Width(95)))
                        {
                            ShareMod.PlayButtonSound();
                            Selected = world;
                            DownloadWorld(world);
                        }
                    }
                    EndHorizontal();

                    Space(5);
                    //------------

                    GUI.enabled = false;

                    if (!WorldTimes.TryGetValue(world, out var time))
                        WorldTimes[world] = time = "";

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
                Selected = world;
            }
        }

        private void UpdateWorldTimes()
        {
            WorldTimes?.Clear();
            
            if (Worlds == null)
            {
                return;
            }

            int time = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            foreach (var item in Worlds)
            {
                WorldTimes[item] = GetTimeString(item.CreatedAt, time);
            }
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

        public void Refresh(bool resetPage = false, bool clearCache = false)
        {
            CurrentState = State.Loading;

            if (resetPage)
                CurrentPage = 0;

            if (clearCache)
                PageCache.Clear();

            LastResponse = null;

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
                else
                {
                    CurrentState = State.Idle;
                }

                ScrollPos = Vector2.zero;

                foreach (var item in Worlds)
                {
                    if (Directory.Exists(SavesPath + item.Title))
                    {
                        WorldStates[item] = WorldState.Existing;
                    }
                    else
                    {
                        WorldStates[item] = WorldState.None;
                    }

                    Remote.User.GetByID(item.AuthorID);
                }

                UpdateWorldTimes();
            }).Start();
        }

        private void DownloadWorld(WorldModel world)
        {
            WorldStates[world] = WorldState.Downloading;

            Remote.DownloadWorld(world.ID, o =>
            {
                SaveWorld(world, o);
                WorldStates[world] = WorldState.Existing;

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
