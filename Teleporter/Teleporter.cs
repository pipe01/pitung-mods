using System.Linq;
using UnityEngine.SceneManagement;
using PiTung.Console;
using PiTung;
using System;
using UnityEngine;

namespace Teleporter
{
    public class Teleporter : Mod
    {
        public override string Name => "Teleporter";
        public override string PackageName => "me.pipe01.Teleporter";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version(1, 0, 0);
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public override void BeforePatch()
        {
            ModInput.RegisterBinding(this, "Teleport", KeyCode.R).ListenKeyDown(Teleport);
        }

        private void Teleport()
        {
            var fps = UnityEngine.Object.FindObjectOfType<FirstPersonInteraction>();
            var trans = fps.PublicFirstPersonCamera.transform;

            if (Physics.Raycast(trans.position, trans.forward, out RaycastHit hit))
            {
                var player = SceneManager.GetActiveScene().GetRootGameObjects().Where(o => o.name.Equals("FPSController")).FirstOrDefault();

                player.transform.position = hit.point;
            }
        }
    }
}
