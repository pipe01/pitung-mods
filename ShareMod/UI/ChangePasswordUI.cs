using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace ShareMod.UI
{
    internal class ChangePasswordUI : UIScreen
    {
        private enum EState
        {
            Idle,
            Loading
        }

        public event EventHandler Done;

        private Rect WindowRect = new Rect(0, 0, 200, 124);
        private string Password = "", Confirm = "";
        private EState State;
        private bool FocusPassword = true;

        public ChangePasswordUI(Remote remote) : base(remote)
        {
        }

        public override void Init()
        {
            WindowRect = WindowRect.CenterInScreen();
        }

        public override void Draw()
        {
            if (!Visible)
                return;

            WindowRect = GUI.Window(3573, WindowRect, DrawWindow, "Change your password");
        }

        private void DrawWindow(int id)
        {
            BeginVertical();
            {
                GUI.enabled = State == EState.Idle;

                BeginHorizontal();
                {
                    Label("Password:");
                    GUI.SetNextControlName("txtPassword");
                    Password = PasswordField(Password, '*', 40, MaxWidth(WindowRect.width / 2));
                }
                EndHorizontal();

                if (FocusPassword)
                {
                    FocusPassword = false;

                    GUI.FocusControl("txtPassword");
                }

                BeginHorizontal();
                {
                    Label("Confirm:");
                    GUI.SetNextControlName("txtConfirm");
                    Confirm = PasswordField(Confirm, '*', 40, MaxWidth(WindowRect.width / 2));
                }
                EndHorizontal();

                string focused = GUI.GetNameOfFocusedControl();

                if (Button("Submit") ||
                    Event.current.isKey && Event.current.keyCode == KeyCode.Return && focused == "txtConfirm")
                {
                    ShareMod.PlayButtonSound();

                    if (!Password.Equals(Confirm))
                    {
                        MessageBox.Show("The passwords must match!", "Error");
                    }
                    else
                    {
                        Submit();
                    }
                }

                if (Button("Cancel"))
                {
                    ShareMod.PlayButtonSound();

                    Password = "";
                    Confirm = "";

                    Done?.Invoke(this, EventArgs.Empty);
                    Hide();
                }

                GUI.enabled = true;
            }
            EndVertical();
        }

        private void Submit()
        {
            new Thread(() =>
            {
                State = EState.Loading;

                string r = Remote.User.ChangePassword(Password, Confirm);
                string msg;

                if (r == "ok")
                {
                    msg = "Password changed successfully!";
                    Hide();
                    Done?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    msg = char.ToUpper(r[0]) + r.Substring(1) + ".";
                }

                MessageBox.Show(msg, r == "ok" ? "Done" : "Error");

                Confirm = "";
                State = EState.Idle;
            }).Start();
        }
    }
}
