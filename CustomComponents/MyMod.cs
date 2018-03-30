using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomComponents
{
    public class MyMod : Mod
    {
        public override string Name => "Components";
        public override string PackageName => "me.pipe01.Components";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        public override void BeforePatch()
        {
            IGConsole.RegisterCommand<Command_add>(this);
        }
    }

    public class Command_add : Command
    {
        public override string Name => "add";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            GetScale();
            
            var clone = CreatePrefabFromCode();
            clone.transform.position = GetPosition();
            
            clone.AddComponent<ObjectInfo>().ComponentType = ComponentType.Delayer;

            clone.transform.position = new Vector3(clone.transform.position.x + 10, clone.transform.position.y, clone.transform.position.z + 10);

            ModUtilities.ExecuteStaticMethod(typeof(StuffPlacer), "SetStateOfAllBoxCollidersFromThingBeingPlaced", true);
            StuffPlacer.OutlinesOfThingBeingPlaced = null;
            
            ModUtilities.SetStaticFieldValue(typeof(StuffPlacer), "BoxCollidersOfThingBeingPlaced", null);

            FloatingPointRounder.RoundIn(clone, true);

            MegaMeshManager.AddComponentsIn(clone);

            foreach (VisualUpdaterWithMeshCombining visualUpdaterWithMeshCombining in clone.GetComponentsInChildren<VisualUpdaterWithMeshCombining>())
            {
                visualUpdaterWithMeshCombining.AllowedToCombineOnStable = true;
            }
            SnappingPeg.TryToSnapIn(clone);
            
            return true;
        }
        
        private Vector3 GetScale()
        {
            if (Physics.Raycast(FirstPersonInteraction.Ray(), out var hit))
            {
                IGConsole.Log(hit.transform.localPosition);
                IGConsole.Log(hit.transform.localScale);
                IGConsole.Log(hit.transform.localEulerAngles);

                ModUtilities.Graphics.CreateSphere(hit.point);
            }

            return Vector3.back;
        }

        private Vector3 GetPosition()
        {
            //if (Physics.Raycast(FirstPersonInteraction.Ray(), out var hit))
            //{
            //    return hit.point;
            //}
            
            return new Vector3(0, 0, 0);
        }

        private static GameObject CreatePrefabFromCode()
        {
            GameObject PrefabRoot = Object.Instantiate(References.Prefabs.WhiteCube, new Vector3(-1000, -1000, -1000), Quaternion.identity);
            PrefabRoot.transform.localScale = new Vector3(.3f, .3f, .3f);

            MegaMeshManager.RemoveComponentImmediatelyOf(PrefabRoot);
            Object.Destroy(PrefabRoot.GetComponent<MegaMeshComponent>());
            PrefabRoot.GetComponent<Renderer>().enabled = false;

            //GameObject Peg = Object.Instantiate(References.Prefabs.Peg, PrefabRoot.transform);
            //Peg.transform.localPosition = new Vector3(0, 1, 0);
            //Peg.transform.localScale = new Vector3(.3f, .8f, .3f);

            AddOutput(0);
            AddOutput(1);
            AddOutput(2);
            AddOutput(3);
            AddOutput(4);
            AddOutput(5);

            PrefabRoot.AddComponent<MyComponent>();

            return PrefabRoot;

            void AddOutput(int side)
            {
                float x = 0, y = 0.5f, z = 0, rotX = 0, rotY = 0, rotZ = 0;

                if (side == 0)
                {
                    z = -0.5f;
                    rotX = 270;
                }
                else if (side == 1)
                {
                    x = -0.5f;
                    rotZ = 90;
                }
                else if (side == 2)
                {
                    z = 0.5f;
                    rotX = 90;
                }
                else if (side == 3)
                {
                    x = 0.5f;
                    rotZ = 270;
                }
                else if (side == 4)
                {
                    y = 1f;
                    rotY = 90;
                }

                //GameObject Output = Object.Instantiate(References.Prefabs.Output, PrefabRoot.transform);
                //Output.transform.localPosition = new Vector3(x, y, z);
                //Output.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
                //Output.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
                GameObject Peg = Object.Instantiate(References.Prefabs.Peg, PrefabRoot.transform);
                Peg.transform.localPosition = new Vector3(x, y, z);
                Peg.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
                Peg.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);

                //var comp = Output.GetComponent<CircuitOutput>();
                //comp.On = true;
                //comp.RecalculateCombinedMesh();
            }
        }
    }

    public class MyComponent : CircuitLogicComponent
    {
        private CircuitInput Input;
        private CircuitOutput Output;

        protected override void OnAwake()
        {
            Input = GetComponentInChildren<CircuitInput>();
            Input.CircuitLogicComponent = this;

            Output = GetComponentInChildren<CircuitOutput>();
        }

        protected override void CircuitLogicUpdate()
        {
            Output.On = !Input.On;
        }
    }
}
