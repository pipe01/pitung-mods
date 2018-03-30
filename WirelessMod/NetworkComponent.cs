using PiTung.Components;
using System.Linq;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace TestMod
{
    public abstract class NetworkComponent : UpdateHandler
    {
        [SaveThis]
        public int Frequency { get; set; } = InteractNetwork.LastFrequency;

        public virtual void UpdateFrequency() { }
    }

    public class InteractNetwork : Interactable
    {
        private Rect WindowRect = new Rect(0, 0, 200, 80);
        private bool Visible = false, ShallFocus = false;
        private string CurrentInput = "";

        public static int LastFrequency = 0;

        public NetworkComponent Component;

        public override void Interact()
        {
            CurrentInput = Component.Frequency.ToString();
            Visible = true;
            ShallFocus = true;

            GameplayUIManager.UIState = UIState.ChooseDisplayColor; //Because this is a display, right?
        }

        void Awake()
        {
            this.gameObject.tag = "Interactable";
        }

        void OnGUI()
        {
            if (Visible)
            {
                WindowRect = new Rect(Screen.width / 2 - WindowRect.width / 2, Screen.height / 2 - WindowRect.height / 2, WindowRect.width, WindowRect.height);

                GUI.Window(28347, WindowRect, DrawWindow, "Receiver setup");
            }

            void DrawWindow(int id)
            {
                BeginVertical();
                {
                    BeginHorizontal();
                    {
                        Label("Frequency:");
                        FlexibleSpace();

                        GUI.SetNextControlName("txtFrq");
                        CurrentInput = TextField(CurrentInput, Width(WindowRect.width / 2));

                        if (ShallFocus)
                        {
                            ShallFocus = false;
                            GUI.FocusControl("txtFrq");
                        }

                        CurrentInput = GetNumbers(CurrentInput);

                        int.TryParse(CurrentInput, out int freq);
                        Component.Frequency = freq;
                        Component.UpdateFrequency();
                    }
                    EndHorizontal();

                    Space(5);

                    if (Button("Done") || Input.GetKeyDown(KeyCode.Return))
                    {
                        LastFrequency = Component.Frequency;
                        Visible = false;
                        GameplayUIManager.UIState = UIState.None;
                    }
                }
                EndVertical();

                GUI.DragWindow();
            }
        }

        private static string GetNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
    }
}
