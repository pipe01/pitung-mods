using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace ShareMod.UI
{
    internal class AccountUI : UIScreen
    {
        private enum EState
        {
            Idle,
            LoggingOut,
            ChangingPassword
        }

        public override bool RequireMainMenu => true;
        
        private Rect WindowRect = new Rect(0, 0, 200, 200);
        private EState State;
        private ChangePasswordUI PasswordUI;

        public AccountUI(Remote remote) : base(remote)
        {
            WindowRect = WindowRect.CenterInScreen();

            PasswordUI = new ChangePasswordUI(remote);
            PasswordUI.Done += (a, b) => State = EState.Idle;
        }

        public override void Init()
        {
            PasswordUI.Init();

            base.Init();
        }

        public override void Draw()
        {
            if (!Remote.User.IsLoggedIn || !RunMainMenu.Instance.MainMenuCanvas.enabled)
                return;

            if (State == EState.ChangingPassword)
            {
                PasswordUI.Draw();
            }
            else
            {
                DrawMainButton();

                if (Visible)
                {
                    WindowRect = Window(12323, WindowRect, DrawWindow, "Your account");
                }
            }
        }

        private void DrawMainButton()
        {
            if (GUI.Button(new Rect(2, 37, 150, 25), "Your account"))
            {
                ShareMod.PlayButtonSound();

                Visible = !Visible;
                SetMainMenuPlaying(!Visible);
            }
        }

        private void DrawWindow(int id)
        {
            BeginVertical();
            {
                if (Button("Change password", Height(30)))
                {
                    ShareMod.PlayButtonSound();

                    State = EState.ChangingPassword;
                    PasswordUI.Show();

                    Hide();
                }

                FlexibleSpace();

                GUI.enabled = State == EState.Idle;
                if (Button("Log out", Height(30)))
                {
                    ShareMod.PlayButtonSound();

                    Logout();
                }
                GUI.enabled = true;

                if (Button("Close"))
                {
                    ShareMod.PlayButtonSound();

                    Hide();
                }
            }
            EndVertical();


            GUI.DragWindow();
        }


        private void Logout()
        {
            new Thread(() =>
            {
                State = EState.LoggingOut;

                if (Remote.User.Logout())
                    Hide();
                
                State = EState.Idle;
            }).Start();
        }
    }
}
