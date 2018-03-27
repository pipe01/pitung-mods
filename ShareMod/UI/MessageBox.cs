using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace ShareMod.UI
{
    internal class MessageBox
    {
        private string Text;
        private string Title;
        private string[] Options;
        private int Result;
        private GUIStyle Style;

        private event EventHandler ResultSet;

        public static IList<MessageBox> Shown = new List<MessageBox>();

        public static void Show(string text, string title, Action<int> result = null, params string[] options)
        {
            options = options.Length == 0 ? new[] { "OK" } : options;

            var msg = new MessageBox(text, title, options);
            Shown.Add(msg);

            msg.ResultSet += (a, b) =>
            {
                Shown.Remove(msg);
                result(msg.Result);
            };
        }

        private MessageBox(string text, string title, string[] options)
        {
            this.Text = text;
            this.Title = title;
            this.Options = options;

            this.Style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            };
        }

        public void Draw()
        {
            if (!Shown.Contains(this))
                return;

            Vector2 size = new Vector2(300, 100);
            Vector2 pos = new Vector2(Screen.width / 2 - size.x / 2, Screen.height / 2 - size.y / 2);

            GUI.Window(123123, new Rect(pos, size), Window, this.Title);

            void Window(int id)
            {
                BeginVertical();
                {
                    FlexibleSpace();

                    Label(this.Text, Style);

                    FlexibleSpace();

                    BeginHorizontal();
                    {
                        FlexibleSpace();

                        int i = 0;
                        foreach (var item in this.Options)
                        {
                            if (Button(item, Width(90), Height(30)))
                            {
                                ShareMod.PlayButtonSound();

                                this.Result = i;
                                ResultSet?.Invoke(this, null);
                            }

                            i++;
                        }

                        FlexibleSpace();
                    }
                    EndHorizontal();
                }
                EndVertical();
            }
        }
    }
}
