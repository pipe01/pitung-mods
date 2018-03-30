using System.Xml;
using System.Reflection;
using System.Linq;
using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;
using UnityEngine;
using PiTung.Config_menu;

namespace ConsoleMod
{
    public class MyMod : Mod
    {
        public override string Name => "Update rate-er";
        public override string PackageName => "me.pipe01.RateEr";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.2.0");
        public override bool Hotloadable => true;
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";
        
        internal static float CurrentUpdateRate = 100;
        internal static Action<float> SetUpdateRate;
        private static bool DrawRate = true;
        private static float StartTime = -1;

        private const float Step = 10f;
        private const float SmallStep = .1f;

        public override void BeforePatch()
        {
            IGConsole.RegisterCommand<CommandRate>(this);

            ModInput.RegisterBinding(this, "RateIncrement", KeyCode.KeypadPlus).ListenKeyDown(Increment);
            ModInput.RegisterBinding(this, "RateDecrement", KeyCode.KeypadMinus).ListenKeyDown(Decrement);
        }

        private void Increment()
        {
            if (ModUtilities.IsOnMainMenu)
                return;

            if (CurrentUpdateRate < 1)
            {
                SetUpdateRate(CurrentUpdateRate + SmallStep);
            }
            else if (CurrentUpdateRate < 10)
            {
                SetUpdateRate(CurrentUpdateRate + 1);
            }
            else
            {
                SetUpdateRate(CurrentUpdateRate + Step);
            }

            DrawRate = true;
            StartTime = Time.time;
        }

        private void Decrement()
        {
            if (CurrentUpdateRate < SmallStep || ModUtilities.IsOnMainMenu)
                return;

            if (CurrentUpdateRate <= 1)
            {
                SetUpdateRate(CurrentUpdateRate - SmallStep);
            }
            else if (CurrentUpdateRate <= 10)
            {
                SetUpdateRate(CurrentUpdateRate - 1);
            }
            else
            {
                SetUpdateRate(CurrentUpdateRate - Step);
            }

            DrawRate = true;
            StartTime = Time.time;
        }
        
        public override void OnGUI()
        {
            if (!ModUtilities.IsOnMainMenu && DrawRate)
            {
                if (StartTime == -1)
                {
                    StartTime = Time.time;
                }
                else if (Time.time - StartTime > 3)
                {
                    DrawRate = false;
                    StartTime = -1;
                }

                ModUtilities.Graphics.DrawText($"Update rate: {CurrentUpdateRate:0.0}", new Vector2(5, 5), Color.green);
            }
        }

        public override IEnumerable<MenuEntry> GetMenuEntries()
        {
            var num = new SimpleNumberEntry(1, 0.1f, 500, 100) { Text = "Update rate" };
            num.ValueChanged += o => SetUpdateRate?.Invoke(o);

            yield break;
        }
    }

    [Target(typeof(BehaviorManager))]
    public static class BehaviorManagerPatch
    {
        [PatchMethod]
        static void Awake(BehaviorManager __instance)
        {
            MyMod.SetUpdateRate = rate =>
            {
                CustomFixedUpdate updateDelegate =
                    ModUtilities.GetFieldValue<CustomFixedUpdate>(__instance, "CircuitLogicUpdate");

                updateDelegate.updateRate = MyMod.CurrentUpdateRate = rate;
            };
        }
    }

    public class CommandRate : Command
    {
        public override string Name => "rate";
        public override string Usage => "rate [tps]";
        public override string Description => "Sets or gets the current update rate (measured in ticks per second)";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (MyMod.SetUpdateRate == null)
            {
                IGConsole.Error("This command can only be used while in-game!");
                return true;
            }

            if (!arguments.Any())
            {
                IGConsole.Log($"<b>Current update rate:</b> {MyMod.CurrentUpdateRate} tps");
                return true;
            }

            string arg = arguments.First();

            if (float.TryParse(arg, out float num))
            {
                MyMod.SetUpdateRate(num);
                return true;
            }
            else
            {
                IGConsole.Error("tps must be a decimal number");
            }

            return false;
        }
    }
}
