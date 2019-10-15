using PiTung;
using References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace LittleAddons
{
    public class LittleAddons : Mod
    {
        public override string Name => "LittleAddons";
        public override string PackageName => "me.pipe01.LittleAddons";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.5");
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public override void BeforePatch()
        {
            ThroughInverter.Register();
            KeyButton.Register();
            TFlipFlop.Register();
            DLatch.Register();
            FileReader.Register();
            EdgeDetector.Register();
            GateAND.Register();
            GateANDB.Register();
            GateAND4.Register();
            GateNAND.Register();
            GateOR.Register();
            GateXOR.Register();
            GateXNOR.Register();

            ModInput.RegisterBinding("DeleteWholeBoard", KeyCode.T, KeyModifiers.Control | KeyModifiers.Shift)
                .ListenKeyDown(DeleteBoard);
        }
        
        private void DeleteBoard()
        {
            if (Physics.Raycast(FirstPersonInteraction.Ray(), out var raycastHit, Settings.ReachDistance, 769) && StuffDeleter.AllowedToDoDeleting && raycastHit.collider.gameObject.tag == "CircuitBoard")
            {
                var board = raycastHit.collider.gameObject;

                BoardFunctions.SetMostRecentlyDeletedBoard(board);

                BoardFunctions.DestroyAllWiresConnectedToBoardButNotPartOfIt(board);
                MegaMeshManager.RemoveComponentsImmediatelyIn(board);

                CircuitInput[] componentsInChildren = board.GetComponentsInChildren<CircuitInput>();
                CircuitOutput[] componentsInChildren2 = board.GetComponentsInChildren<CircuitOutput>();

                foreach (CircuitInput input in componentsInChildren)
                {
                    StuffDeleter.DestroyInput(input);
                }

                foreach (CircuitOutput output in componentsInChildren2)
                {
                    StuffDeleter.DestroyOutput(output);
                }

                GameObject.Destroy(board);

                SoundPlayer.PlaySoundGlobal(Sounds.DeleteSomething);
            }
        }
        
        internal static void PlayButtonSound()
        {
            SoundPlayer.PlaySoundGlobal(Sounds.UIButton);
        }
    }
}
