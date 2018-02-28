using UnityEngine.SceneManagement;
using System.Collections;
using System;
using PiTung;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CopyPasteMod
{
    public class CopyPasteMod : Mod
    {
        public override string Name => "Copy Paste";
        public override string PackageName => "me.pipe01.CopyPaste";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";
        
        private GameObject ClipboardBoard;

        public CopyPasteMod()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        public override void BeforePatch()
        {
            ModInput.RegisterBinding(this, "Copy", KeyCode.G, KeyModifiers.Shift)
                .ListenKeyDown(Copy);

            ModInput.RegisterBinding(this, "Paste", KeyCode.G)
                .ListenKeyDown(Paste);
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            Clear();
        }
        
        private void Clear()
        {
            if (ClipboardBoard == null)
                return;

            MegaBoardMeshManager.RemoveBoardsFrom(ClipboardBoard);
            MegaMesh.RemoveMeshesFrom(ClipboardBoard);
            Object.Destroy(ClipboardBoard);
            ClipboardBoard = null;
        }
        
        private void Copy()
        {
            var board = GetTargetBoard();

            if (board != null)
            {
                ClipboardBoard = Object.Instantiate(board, new Vector3(0, 123456f, 0), Quaternion.identity);
            }
            else
            {
                Clear();
            }
        }
        
        private void Paste()
        {
            if (ClipboardBoard != null)
            {
                BoardPlacer.BoardBeingPlaced = Object.Instantiate(ClipboardBoard, new Vector3(0f, -2000f, 0f), Quaternion.identity);
                
                foreach (WireCluster wireCluster in BoardPlacer.BoardBeingPlaced.GetComponentsInChildren<WireCluster>())
                {
                    Object.Destroy(wireCluster.gameObject);
                }
                foreach (CircuitBoard circuitBoard in BoardPlacer.BoardBeingPlaced.GetComponentsInChildren<CircuitBoard>())
                {
                    circuitBoard.Renderer.enabled = true;
                }

                BoardPlacer.Instance.RecalculateClustersOfCurrentBoard();
                ModUtilities.ExecuteMethod(BoardPlacer.Instance, "NewBoardBeingPlaced");
                ModUtilities.DummyComponent.StartCoroutine(BoardBeingPlaced());
            }

            IEnumerator BoardBeingPlaced()
            {
                yield return new WaitForSeconds(0.1f);
                ModUtilities.ExecuteMethod(BoardPlacer.Instance, "NewBoardBeingPlaced");
            }
        }
        
        private GameObject GetTargetBoard()
        {
            var transform = FirstPersonInteraction.FirstPersonCamera.transform;

            if (Physics.Raycast(transform.position, transform.forward, out var hit, MiscellaneousSettings.ReachDistance) && hit.transform.gameObject.GetComponent<CircuitBoard>() != null)
            {
                return hit.transform.gameObject;
            }

            return null;
        }
    }
}
