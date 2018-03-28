using System;
using System.Threading;
using System.Web.UI.WebControls;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace ShareMod.UI
{
    internal class LoginUI : UIScreen
    {
        private enum EState
        {
            Idle,
            LoggingIn,
            Registering
        }

        public override bool RequireMainMenu => true;

        private Rect LoginWindowRect = new Rect(100, 100, 200, 124);
        private string Username = "", Password = "", RegisterResult = "";
        private bool ShallFocusUsername;
        private EState State;

        public LoginUI(Remote remote) : base(remote)
        {
        }
        
        public override void Draw()
        {
            if (Remote.User.IsLoggedIn)
                return;

            string text = Remote.User.IsLoggedIn ? "Log out" : "Log in to ShareMod";

            GUI.enabled = State != EState.LoggingIn;
            bool b = GUI.Button(new Rect(2, 37, 150, 25), text);
            GUI.enabled = true;

            if (b)
            {
                ShareMod.PlayButtonSound();

                if (!Remote.User.IsLoggedIn)
                {
                    if (State != EState.LoggingIn)
                    {
                        Visible = !Visible;
                        SetMainMenuPlaying(!Visible);

                        if (Visible)
                        {
                            ShallFocusUsername = true;
                        }
                    }
                }
            }

            if (Visible && Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }

            if (Visible)
            {
                LoginWindowRect = LoginWindowRect.CenterInScreen();
                GUI.Window(1234, LoginWindowRect, new GUI.WindowFunction(MyWindow), "ShareMod");
            }

            void MyWindow(int id)
            {
                BeginVertical();
                {
                    GUI.enabled = State != EState.LoggingIn && State != EState.Registering;

                    BeginHorizontal();
                    {
                        GUILayout.Label("Username:");
                        GUI.SetNextControlName("txtUsername");
                        Username = TextField(Username, 40, MaxWidth(LoginWindowRect.width / 2));
                    }
                    EndHorizontal();

                    BeginHorizontal();
                    {
                        GUILayout.Label("Password:");
                        GUI.SetNextControlName("txtPassword");
                        Password = PasswordField(Password, '*', 40, MaxWidth(LoginWindowRect.width / 2));
                    }
                    EndHorizontal();

                    GUI.enabled = true;

                    if (ShallFocusUsername)
                    {
                        ShallFocusUsername = false;

                        GUI.FocusControl("txtUsername");
                    }

                    
                    if (State == EState.LoggingIn)
                    {
                        GUILayout.Label("Logging in...", new GUIStyle
                        {
                            normal = new GUIStyleState
                            {
                                textColor = new Color(0, 200, 0)
                            }
                        });
                    }
                    else if (State == EState.Registering)
                    {
                        GUILayout.Label("Registering...", new GUIStyle
                        {
                            normal = new GUIStyleState
                            {
                                textColor = new Color(0, 200, 0)
                            }
                        });
                    }
                    else
                    {
                        bool login = false;

                        if (Button("Login", MaxWidth(LoginWindowRect.width)))
                        {
                            ShareMod.PlayButtonSound();
                            login = true;
                        }
                        else if (Button("Register", MaxWidth(LoginWindowRect.width)))
                        {
                            ShareMod.PlayButtonSound();

                            Register(Username, Password);

                            Username = "";
                            Password = "";
                        }

                        string focused = GUI.GetNameOfFocusedControl();
                        if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && (focused == "txtUsername" || focused == "txtPassword"))
                        {
                            login = true;
                        }

                        if (login)
                        {
                            Login(Username, Password);

                            Password = "";
                        }
                    }
                }
                EndVertical();

                GUI.DragWindow();
            }
        }

        private void Login(string username, string password)
        {
            new Thread(() =>
            {
                State = EState.LoggingIn;

                string result = Remote.User.Login(username, password);

                if (result == "ok")
                {
                    Hide();
                }
                else
                {
                    result = char.ToUpper(result[0]) + result.Substring(1) + ".";

                    MessageBox.Show(result, "Error");
                }

                State = EState.Idle;
            }).Start();
        }

        private void Register(string username, string password)
        {
            new Thread(() =>
            {
                State = EState.Registering;

                RegisterResult = Remote.User.Register(username, password);
                Console.WriteLine(RegisterResult);

                if (RegisterResult == "ok")
                {
                    Login(username, password);
                }
                else
                {
                    RegisterResult = char.ToUpper(RegisterResult[0]) + RegisterResult.Substring(1) + ".";

                    MessageBox.Show(RegisterResult, "Error", o => State = EState.Idle);
                    Username = username;
                }
            }).Start();
        }
    }
}
