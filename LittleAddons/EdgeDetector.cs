using PiTung.Components;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LittleAddons
{
    public class EdgeDetector : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Back, SideType.Input)
                .WithIO(CubeSide.Front, SideType.Output)
                .WithColor(Color.yellow)
                .WithComponent<EdgeInteract>();

            ComponentRegistry.CreateNew<EdgeDetector>("edgedetector", "Edge Detector", b);
        }

        [SaveThis]
        public int PulseLength { get; set; } = 2;

        [SaveThis]
        public bool FallingEdge { get; set; } = true;

        [SaveThis]
        public bool RisingEdge { get; set; } = true;

        private bool LastState = false;
        private int TickCounter = 0;

        protected override void OnAwake()
        {
            this.tag = "Interactable";
        }

        protected override void CircuitLogicUpdate()
        {
            bool input = Inputs[0].On;

            if (Outputs[0].On)
            {
                TickCounter++;

                if (TickCounter >= PulseLength)
                {
                    TickCounter = 0;
                    Outputs[0].On = false;
                }
                else
                {
                    this.ContinueUpdatingForAnotherTick();
                }
            }
            else if (input != LastState)
            {
                LastState = input;

                if ((input && RisingEdge) || (!input && FallingEdge))
                {
                    Outputs[0].On = true;

                    this.ContinueUpdatingForAnotherTick();
                }
            }
        }

        private class EdgeInteract : Interactable
        {

            public bool Visible { get; set; }

            private Rect WindowRect = new Rect(0, 0, 200, 140);
            private EdgeDetector EdgeDetector;

            public override void Interact()
            {
                Visible = true;
                EdgeDetector = GetComponent<EdgeDetector>();
                
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

                GUI.Window(34712, WindowRect, DrawWindow, "Edge detector behavior");
            }

            private void DrawWindow(int id)
            {
                BeginVertical();
                {
                    BeginHorizontal();
                    {
                        Label("Pulse length: ");
                        string str = TextField(EdgeDetector.PulseLength.ToString(), 3);
                        EdgeDetector.PulseLength = int.Parse(new string(str.Where(Char.IsDigit).ToArray()));
                    }
                    EndHorizontal();

                    EdgeDetector.PulseLength = (int)HorizontalSlider(EdgeDetector.PulseLength, 1, 20);

                    Label("Pulse on:");
                    BeginHorizontal();
                    {
                        EdgeDetector.RisingEdge = Toggle(EdgeDetector.RisingEdge, "Rising edge");
                        EdgeDetector.FallingEdge = Toggle(EdgeDetector.FallingEdge, "Falling edge");
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
            }

            private void Close()
            {
                Visible = false;
                GameplayUIManager.UIState = UIState.None;
            }
        }
    }
}
