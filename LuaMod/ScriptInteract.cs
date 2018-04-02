using References;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LuaMod
{
    internal class ScriptInteract : Interactable
    {
        private bool Visible = false;
        private CpuComponent Lua;
        private string Buffer = "";

        private static GUIStyle ErrorStyle = new GUIStyle
        {
            normal = new GUIStyleState
            {
                textColor = Color.red
            },
            fontSize = 15
        };
        private static GUIStyle EditorStyle = new GUIStyle
        {
            normal = new GUIStyleState
            {
                textColor = Color.gray
            },
            font = Font.CreateDynamicFontFromOSFont("Consolas", 15)
        };
        private static Rect WindowRect = Rect.zero;
        

        void Awake()
        {
            this.gameObject.tag = "Interactable";
            this.Lua = GetComponent<CpuComponent>();
            this.Buffer = this.Lua.LuaScript;

            if (WindowRect == Rect.zero)
                WindowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400);
        }

        public override void Interact()
        {
            Visible = true;
            GameplayUIManager.UIState = UIState.ChooseDisplayColor;
        }

        void OnGUI()
        {
            if (!Visible)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameplayUIManager.UIState = UIState.None;
                Visible = false;
            }

            WindowRect = GUI.Window(23897, WindowRect, DrawWindow, "Edit Lua script");
        }

        private void DrawWindow(int id)
        {
            BeginVertical();
            {
                this.Buffer = TextArea(this.Buffer, EditorStyle, MaxWidth(WindowRect.width - 5), MaxHeight(WindowRect.height - 5));

                BeginHorizontal();
                {
                    if (!string.IsNullOrEmpty(Lua.CurrentError))
                        Label("Error: " + Lua.CurrentError, ErrorStyle);

                    FlexibleSpace();

                    if (Button("Save"))
                    {
                        SoundPlayer.PlaySoundGlobal(Sounds.UIButton);

                        Visible = false;

                        Submit();
                    }
                }
                EndHorizontal();
            }
            EndVertical();
        }

        private void Submit()
        {
            GameplayUIManager.UIState = UIState.None;
            Lua.LuaScript = Buffer;
        }
    }
}
