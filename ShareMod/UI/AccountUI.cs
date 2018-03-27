using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace ShareMod.UI
{
    internal class AccountUI : UIScreen
    {
        public override bool RequireMainMenu => true;

        private bool ShowWindow = false;
        private Rect WindowRect = new Rect(0, 0, 200, 200);

        public AccountUI(Remote remote) : base(remote)
        {
            WindowRect.Set(Screen.width / 2 - WindowRect.width / 2, Screen.height / 2 - WindowRect.height / 2, WindowRect.width, WindowRect.height);
        }

        public override void Draw()
        {
            DrawMainButton();
        }

        private void DrawMainButton()
        {
            if (GUI.Button(new Rect(2, 37, 150, 25), "Your account"))
            {
                ShareMod.PlayButtonSound();

                ShowWindow = !ShowWindow;

                SetMainMenuPlaying(!ShowWindow);
            }

            if (ShowWindow)
            {
                WindowRect = Window(12323, WindowRect, DrawWindow, "Your account");
            }
        }

        private void DrawWindow(int id)
        {
            if (Button("Log out", Height(30)))
            {
                ShareMod.PlayButtonSound();


            }

            GUI.DragWindow();
        }
    }
}
