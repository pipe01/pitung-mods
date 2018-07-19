using PiTung;
using PiTung.Console;
using References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UndoMod
{
    public class UndoMod : Mod
    {
        public override string Name => "UndoMod";
        public override string PackageName => "me.pipe01.UndoMod";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        public static Stack<UndoItem> UndoStack { get; } = new Stack<UndoItem>();

        public override void BeforePatch()
        {
            ModInput.RegisterBinding("Undo", KeyCode.Z, KeyModifiers.Control).ListenKeyDown(Undo);
        }

        private void Undo()
        {
            if (UndoStack.Count == 0)
                return;

            var item = UndoStack.Pop();
            item.Undo();
        }

        public override void OnGUI()
        {
            ModUtilities.Graphics.DrawText("Stack count: " + UndoStack.Count, new Vector2(3, 10), Color.white, true);
        }

        public override void OnWorldLoaded(string worldName)
        {
            UndoStack.Clear();
        }
    }
}
