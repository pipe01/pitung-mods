using PiTung.Components;
using PiTung.Console;
using PiTung.Mod_utilities;
using References;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LittleAddons
{
    public class FileReader : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Top, SideType.Input, "Clock")
                .WithIO(CubeSide.Left, SideType.Input, "Reset")
                .WithIO(CubeSide.Front, SideType.Output, "Data")
                .WithIO(CubeSide.Back, SideType.Output, "EOF")
                .WithComponent<FileInteract>();

            ComponentRegistry.CreateNew<FileReader>("filereader", "File Reader", b);
        }

        [SaveThis]
        public string FilePath { get; set; } = "";

        [SaveThis]
        public bool BigEndian { get; set; } = false;

        private BitArray Data;
        private int Index;

        protected override void OnAwake()
        {
            this.tag = "Interactable";
        }

        protected override void CircuitLogicUpdate()
        {
            if (Inputs[0].On)
            {
                if (Data == null)
                    OpenFile();

                if (Outputs[1].On)
                    Outputs[1].On = false;

                Outputs[0].On = Data[Index++];

                if (Index == Data.Length || Inputs[1].On) //EOF
                {
                    Outputs[1].On = true;

                    Reset(false);
                }
            }
        }

        public void Reset(bool setOut)
        {
            Index = 0;
            Data = null;

            if (setOut)
                Outputs[0].On = false;
        }

        private void OpenFile()
        {
            if (!File.Exists(FilePath))
            {
                IGConsole.Error($"Couldn't find file at '{FilePath}'.");
                SoundPlayer.PlaySoundGlobal(Sounds.FailDoSomething);
                return;
            }

            Data = new BitArray(File.ReadAllBytes(FilePath));

            if (BigEndian)
            {
                Data = new BitArray(Data
                   .Cast<bool>()
                   .Select((x, i) => new { Index = i, Value = x })
                   .GroupBy(x => x.Index / 8)
                   .Select(x => x.Select(v => v.Value).Reverse().ToList())
                   .SelectMany(o => o)
                   .ToArray());
            }
        }

        private class FileInteract : Interactable
        {
            private bool Visible = false;
            private Rect WindowRect = new Rect(0, 0, 200, 95);
            private FileReader Reader;
            private string Buffer = "";

            void Start()
            {
                Reader = GetComponent<FileReader>();
                Buffer = Reader.FilePath;
            }

            public override void Interact()
            {
                Visible = true;

                if (Visible)
                    GameplayUIManager.UIState = UIState.ChooseDisplayColor;
                else
                    GameplayUIManager.UIState = UIState.None;
            }

            void OnGUI()
            {
                if (!Visible)
                    return;

                WindowRect.x = Screen.width / 2 - WindowRect.width / 2;
                WindowRect.y = Screen.height / 2 - WindowRect.height / 2;

                GUI.Window(28347, WindowRect, DrawWindow, "Choose a file");
            }

            private void DrawWindow(int id)
            {
                BeginVertical();
                {
                    BeginHorizontal();
                    {
                        Label("File path:");

                        FlexibleSpace();

                        Buffer = TextField(Buffer, Width(WindowRect.width / 2));
                    }
                    EndHorizontal();

                    Reader.BigEndian = Toggle(Reader.BigEndian, "Big endian?");

                    if (Button("Done"))
                    {
                        LittleAddons.PlayButtonSound();

                        Reader.FilePath = Buffer;
                        Reader.Reset(true);

                        Visible = false;
                        GameplayUIManager.UIState = UIState.None;
                    }
                }
                EndVertical();
            }
        }
    }
}
