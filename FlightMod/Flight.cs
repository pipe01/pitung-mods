using System.Reflection;
using UnityEngine;
using PiTung.Console;
using PiTung;
using System;
using UnityStandardAssets.Characters.FirstPerson;

namespace FlightMod
{
    public class Flight : Mod
    {
        public override string Name => "Flight";
        public override string PackageName => "me.pipe01.Flight";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version(1, 1, 2);
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public override void BeforePatch()
        {
            ModInput.RegisterBinding(this, "ToggleFlight", KeyCode.F);
            ModInput.RegisterBinding(this, "ToggleAxisLock", KeyCode.B);
        }
    }

    [Target(typeof(FirstPersonController))]
    public class FirstPersonControllerPatch
    {
        [PatchMethod(PatchType = PatchType.Postfix)]
        static void Start(FirstPersonController __instance)
        {
            var my = __instance.gameObject.AddComponent<MyCharController>();
            my.m_MouseLook = __instance.m_MouseLook;
            my.m_Camera = (Camera)__instance.GetType().GetField("m_Camera", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            my.m_Enabled = false;
        }
    }

    [Target(typeof(UIManager))]
    public class UIManagerPatch
    {
        [PatchMethod]
        static void UnlockMouseAndDisableFirstPersonLooking()
        {
            if (MyCharController.Instance != null)
                MyCharController.Instance.enabled = false;
        }

        [PatchMethod]
        static void LockMouseAndEnableFirstPersonLooking()
        {
            if (MyCharController.Instance != null)
                MyCharController.Instance.enabled = true;
        }
    }
}
