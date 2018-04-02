using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public WorldModel World { get; }

        private Rect WindowRect = new Rect(0, 0, 200, 200);

        public ManageWorldUI(Remote remote, WorldModel world) : base(remote)
        {
            this.World = world;
        }

        public override void Draw()
        {
            if (!Visible)
                return;

            WindowRect = GUI.Window(83457, WindowRect, DrawWindow, this.World.Title);
        }

        private void DrawWindow(int id)
        {
            BeginVertical();
            {
                if (Button("Delete"))
                {
                    ShareMod.PlayButtonSound();

                    MessageBox.Show($"Are you sure you want to delete the world '{World.Title}'?", "u sure bro?", o =>
                    {
                        if (o == 0)
                        {
                            DeleteWorld();
                        }
                    }, "Yes", "No");
                }

                FlexibleSpace();

                if (Button("Close"))
                {
                    ShareMod.PlayButtonSound();

                    Visible = false;
                    Closed?.Invoke(this, EventArgs.Empty);
                }
            }
            EndVertical();
        }

        private void DeleteWorld()
        {

        }
    }
}
