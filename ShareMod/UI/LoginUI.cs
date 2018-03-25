using System.Threading;
using System.Web.UI.WebControls;
using UnityEngine;

namespace ShareMod.UI
{
    internal class LoginUI : UIScreen
    {
        private enum EState
        {
            Idle,
            LoggingIn,
            LoggingOut
        }

        private Rect LoginWindowRect = new Rect(100, 100, 200, 100);
        private string Username = "", Password = "";
        private bool ShallFocusUsername;
        private EState State;

        public LoginUI(Remote remote) : base(remote)
        {
        }
        
        public override void Draw()
        {
            if (!(RunMainMenu.Instance.MainMenuCanvas?.enabled ?? false))
            {
                return;
            }

            string text = Remote.User.IsLoggedIn ? "Log out" : "Log in to ShareMod";

            GUI.enabled = State != EState.LoggingIn && State != EState.LoggingOut;
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

                        if (Visible)
                        {
                            ShallFocusUsername = true;
                        }
                    }
                }
                else if (State != EState.LoggingOut)
                {
                    Logout();
                }
            }

            if (Visible && Input.GetKeyDown(KeyCode.Escape))
            {
                Visible = false;
            }

            if (Visible)
            {
                LoginWindowRect.x = Screen.width / 2 - LoginWindowRect.width / 2;
                LoginWindowRect.y = Screen.height / 2 - LoginWindowRect.height / 2;
                GUI.Window(1234, LoginWindowRect, new GUI.WindowFunction(MyWindow), "ShareMod");
            }

            void MyWindow(int id)
            {
                GUILayout.BeginVertical();

                GUI.enabled = State != EState.LoggingIn;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Username:");
                GUI.SetNextControlName("txtUsername");
                Username = GUILayout.TextField(Username, 40, GUILayout.MaxWidth(LoginWindowRect.width / 2));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Password:");
                GUI.SetNextControlName("txtPassword");
                Password = GUILayout.PasswordField(Password, '*', 40, GUILayout.MaxWidth(LoginWindowRect.width / 2));
                GUILayout.EndHorizontal();

                GUI.enabled = true;

                if (ShallFocusUsername)
                {
                    ShallFocusUsername = false;

                    GUI.FocusControl("txtUsername");
                }

                if (State != EState.LoggingIn)
                {
                    bool login = false;

                    if (GUILayout.Button("Login", GUILayout.MaxWidth(LoginWindowRect.width)))
                    {
                        ShareMod.PlayButtonSound();
                        login = true;
                    }
                    
                    string focused = GUI.GetNameOfFocusedControl();
                    if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && (focused == "txtUsername" || focused == "txtPassword"))
                    {
                        login = true;
                    }

                    if (login)
                    {
                        Login(Username, Password);

                        Username = "";
                        Password = "";
                    }
                }
                else
                {
                    GUILayout.Label("Logging in...", new GUIStyle
                    {
                        normal = new GUIStyleState
                        {
                            textColor = new Color(0, 200, 0)
                        }
                    });
                }

                GUILayout.EndVertical();

                GUI.DragWindow();
            }
        }

        private void Logout()
        {
            new Thread(() =>
            {
                State = EState.LoggingOut;
                Remote.User.Logout();
                State = EState.Idle;
            }).Start();
        }

        private void Login(string username, string password)
        {
            new Thread(() =>
            {
                State = EState.LoggingIn;
                if (Remote.User.Login(username, password))
                {
                    Visible = false;
                }
                State = EState.Idle;
            }).Start();
        }
    }
}
