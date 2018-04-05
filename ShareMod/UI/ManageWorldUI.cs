using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace ShareMod.UI
{
    internal class ManageWorldUI : UIScreen
    {
        private enum EState
        {
            Idle,
            Deleting
        }

        public event EventHandler Closed;
        public event EventHandler Refresh;
        public WorldModel World { get; }

        private Rect WindowRect = new Rect(0, 0, 200, 200);
        private EState State;

        public ManageWorldUI(Remote remote, WorldModel world) : base(remote)
        {
            this.World = world;
            this.WindowRect = new Rect(Screen.width / 2 - WindowRect.width / 2, Screen.height / 2 - WindowRect.height / 2, WindowRect.width, WindowRect.height);
        }

        public override void Draw()
        {
            if (!Visible)
                return;

            WindowRect = GUI.Window(83457, WindowRect, DrawWindow, this.World.Title);
        }

        private void DrawWindow(int id)
        {
            GUI.BringWindowToFront(id);

            BeginVertical();
            {
                GUI.enabled = State != EState.Deleting;

                if (Button(State == EState.Deleting ? "Deleting..." : "Delete"))
                {
                    ShareMod.PlayButtonSound();

                    MessageBox.Show($"Are you sure you want to delete the world '{World.Title}'?", "u sure bro?", o =>
                    {
                        if (o == 0)
                        {
                            DeleteWorld(World);
                        }
                    }, "Yes", "No");
                }

                FlexibleSpace();

                if (Button("Close"))
                {
                    ShareMod.PlayButtonSound();

                    Close();
                }
            }
            EndVertical();

            GUI.DragWindow();
        }

        private void Close()
        {
            Visible = false;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteWorld(WorldModel world)
        {
            new Thread(() =>
            {
                State = EState.Deleting;

                string status = Remote.DeleteWorld(world.ID);

                if (status != "ok")
                {
                    MessageBox.Show($"An error occurred while deleting world: {status}", "Error");
                }
                else
                {
                    MessageBox.Show("World deleted successfully!", "Success");

                    Refresh?.Invoke(this, EventArgs.Empty);
                    Close();
                }

                State = EState.Idle;
            }).Start();
        }
    }
}
