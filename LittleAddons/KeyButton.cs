using PiTung.Components;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LittleAddons
{
    public class KeyButton : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Front, SideType.Output)
                .WithColor(Color.cyan)
                .WithComponent<KeyInteract>();

            ComponentRegistry.CreateNew<KeyButton>("keybutton", "Key Button", b);
        }

        [SaveThis]
        public KeyCode Key { get; set; }

        private bool Value = false;

        protected override void OnAwake()
        {
            this.tag = "Interactable";
        }

        void Update()
        {
            bool input = Input.GetKey(this.Key);

            if (input != Value)
            {
                Value = input;

                this.QueueCircuitLogicUpdate();
            }
        }

        protected override void CircuitLogicUpdate()
        {
            Outputs[0].On = Value;
        }

        public class KeyInteract : Interactable
        {
            public bool Visible { get; set; }

            private Rect WindowRect = new Rect(0, 0, 200, 80);
            private string KeyText = "";
            private KeyButton Button = null;

            public override void Interact()
            {
                Visible = true;
                Button = GetComponent<KeyButton>();

                KeyText = Button.Key.ToString();

                GameplayUIManager.UIState = UIState.ChooseDisplayColor;
            }

            void LateUpdate()
            {
                if (Visible && Input.GetKeyDown(KeyCode.Escape))
                    Close();
            }

            void OnGUI()
            {
                if (!Visible)
                    return;
                
                WindowRect.x = Screen.width / 2 - WindowRect.width / 2;
                WindowRect.y = Screen.height / 2 - WindowRect.height / 2;

                GUI.Window(34712, WindowRect, DrawWindow, "Set key");
            }

            private void DrawWindow(int id)
            {
                BeginVertical();
                {
                    BeginHorizontal();
                    {
                        Label("Key:");

                        FlexibleSpace();

                        GUI.SetNextControlName("txtKey");
                        TextField(KeyText, Width(WindowRect.width / 2));
                    }
                    EndHorizontal();

                    Space(2);

                    if (Button("Done"))
                    {
                        LittleAddons.PlayButtonSound();

                        Close();
                    }
                }
                EndVertical();

                if (GUI.GetNameOfFocusedControl() == "txtKey" && Event.current.isKey)
                {
                    KeyText = Event.current.keyCode.ToString();
                    Button.Key = Event.current.keyCode;
                }
            }

            private void Close()
            {
                Visible = false;
                GameplayUIManager.UIState = UIState.None;
            }
        }
    }
}
