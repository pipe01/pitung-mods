using ICSharpCode.SharpZipLib.Zip;
using PiTung;
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

        private Rect WorldsWindowRect = new Rect(0, 0, 500, 500);
        private Vector2 ScrollPos = new Vector2();
        private WorldModel Selected = null;
        private WorldsResponseModel LastResponse = null;
        private IDictionary<WorldModel, WorldState> WorldStates = new Dictionary<WorldModel, WorldState>();
        private GUIStyle EntryStyle, EntryHoverStyle, WorldInfoStyle;
        private int CurrentPage = 0;

        private WorldModel[] Worlds => LastResponse.Items;

        private string SavesPath => Application.persistentDataPath + "/saves/";

        public StoreUI(Remote remote) : base(remote)
        {
            WorldsWindowRect.x = Screen.width / 2 - WorldsWindowRect.width / 2;
            WorldsWindowRect.y = Screen.height / 2 - WorldsWindowRect.height / 2;
        }
        
        public override void Init()
        {
            base.Init();

            Refresh();

            foreach (var item in Worlds)
            {
                Remote.User.GetByID(item.AuthorID);
            }

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

            WorldInfoStyle = new GUIStyle()
            {
                normal = new GUIStyleState
                {
                    textColor = GUI.contentColor
                },
                padding = new RectOffset(5, 5, 1, 1)
            };
        }

        public override void Draw()
        {
            if (!IsInitialized || !(RunMainMenu.Instance.MainMenuCanvas?.enabled ?? false))
                return;

            if (GUI.Button(new Rect(2, 67, 150, 25), "Browse market"))
            {
                Visible = !Visible;
                ShareMod.PlayButtonSound();
            }

            if (Visible && Input.GetKeyDown(KeyCode.Escape))
            {
                Visible = false;
            }

            if (Visible)
            {
                GUI.Window(1237, WorldsWindowRect, o => { }, "");
                WorldsWindowRect = GUI.Window(1235, WorldsWindowRect, DrawWindow, "World market");
                GUI.BringWindowToFront(1235);
            }

            void DrawWindow(int id)
            {
                GUI.DragWindow(new Rect(0, 0, WorldsWindowRect.width, 25));

                if (LastResponse == null)
                    return;

                BeginVertical();
                {
                    ScrollPos = BeginScrollView(ScrollPos, false, true, MaxWidth(WorldsWindowRect.width));
                    {
                        foreach (var item in Worlds)
                        {
                            DrawWorldEntry(item);
                        }
                    }
                    EndScrollView();

                    BeginHorizontal();
                    {
                        Label($"<size=20>Page {LastResponse.Page}</size>");

                        FlexibleSpace();
                        BeginVertical();
                        {
                            FlexibleSpace();

                            GUI.enabled = LastResponse.Page != 0;
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
                            GUI.enabled = !LastResponse.LastPage;
                            if (Button("Next page", Height(30), Width(95)))
                            {
                                ShareMod.PlayButtonSound();

                                CurrentPage++;
                                Refresh();
                            }
                            GUI.enabled = true;

                            FlexibleSpace();
                        }
                        EndVertical();
                    }
                    EndHorizontal();
                }
                EndVertical();
            }
        }

        private void DrawWorldEntry(WorldModel world)
        {
            string authorName = Remote.User.GetByID(world.AuthorID)?.Username;
            
            BeginHorizontal(world == Selected ? EntryHoverStyle : EntryStyle);

            Box("image", Width(60), Height(60));

            BeginVertical();
            Label($"<size=16>{world.Title}</size>");
            
            Label("by " + authorName, WorldInfoStyle);
            Label("ID: " + world.ID, WorldInfoStyle);
            EndVertical();

            //--Download button--
            var state = WorldStates[world];

            string btnText =
                state == WorldState.Downloading ? "Downloading..." :
                state == WorldState.Existing ? "Installed" : "Download";

            GUI.enabled = state != WorldState.Downloading && state != WorldState.Existing;
            FlexibleSpace();
            BeginVertical();
            FlexibleSpace();
            if (Button(btnText, Height(30), Width(95)))
            {
                ShareMod.PlayButtonSound();
                Selected = world;
                DownloadWorld(world);
            }
            FlexibleSpace();
            EndVertical();
            GUI.enabled = true;
            //------------

            EndHorizontal();

            Rect horRect = GUILayoutUtility.GetLastRect();

            if (Event.current.isMouse && horRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                Selected = world;
            }
        }

        public void Refresh(bool resetPage = false)
        {
            if (resetPage)
                CurrentPage = 0;

            LastResponse = null;
            LastResponse = Remote.GetWorlds(CurrentPage);

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
            }

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
