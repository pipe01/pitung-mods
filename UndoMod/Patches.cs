using Harmony;
using PiTung;
using PiTung.Console;
using SavedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace UndoMod
{
    #region Wire detecting
    [HarmonyPatch(typeof(StuffDeleter), "DestroyWire", new[] { typeof(GameObject) })]
    internal static class StuffDeleterPatch
    {
        public static bool Enabled = true;
        
        static void Prefix(GameObject wire)
        {
            if (!Enabled)
                return;

            var ii = wire.GetComponent<InputInputConnection>();
            var io = wire.GetComponent<InputOutputConnection>();

            if (ii == null && io == null)
            {
                Console.WriteLine("the fuck just happened?");
                return;
            }

            var pointA = ii?.Point1 ?? io.Point1;
            var pointB = ii?.Point2 ?? io.Point2;
            
            UndoMod.UndoStack.Push(
                new WireUndoItem(
                    new Connection(
                        ii == null ? Connection.Ways.InputOutput : Connection.Ways.InputInput,
                        pointA.gameObject,
                        pointB.gameObject
                    )
                )
            );
        }
    }

    [Target(typeof(StuffRotater))]
    internal static class StuffRotaterPatch
    {
        [PatchMethod]
        static void DestroyIntersectingConnections()
        {
            StuffDeleterPatch.Enabled = false;
        }

        [PatchMethod(OriginalMethod = "DestroyIntersectingConnections", PatchType = PatchType.Postfix)]
        static void DestroyIntersectingConnectionsPostfix()
        {
            StuffDeleterPatch.Enabled = true;
        }
    }
    #endregion

    #region Component detecting
    [HarmonyPatch(typeof(StuffDeleter), "RunGameplayDeleting")]
    internal static class DeleteThingPatch
    {
        public static void CheckObject(GameObject gameObject)
        {
            if (gameObject == null || gameObject.tag == "World" || gameObject.tag == "Wire" || gameObject.tag == "CircuitBoard")
            {
                return;
            }
            
            var savedObj = SavedObjectUtilities.CreateSavedObjectFrom(gameObject);
            var undoItem = new ComponentUndoItem
            {
                SavedObject = savedObj,
                Parent = gameObject.transform.parent
            };

            foreach (var input in gameObject.GetComponentsInChildren<CircuitInput>())
            {
                undoItem.Wires.AddRange(input.IIConnections.Cast<Wire>());
                undoItem.Wires.AddRange(input.IOConnections.Cast<Wire>());
            }

            foreach (var input in gameObject.GetComponentsInChildren<CircuitOutput>())
            {
                undoItem.Wires.AddRange(input.GetIOConnections().Cast<Wire>());
            }

            UndoMod.UndoStack.Push(undoItem);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var newInst = new List<CodeInstruction>();
            newInst.Add(new CodeInstruction(OpCodes.Ldloca_S, 0));
            newInst.Add(new CodeInstruction(OpCodes.Call, typeof(RaycastHit).GetProperty("collider").GetAccessors()[0]));
            newInst.Add(new CodeInstruction(OpCodes.Call, typeof(Component).GetProperty("gameObject").GetAccessors()[0]));
            newInst.Add(new CodeInstruction(OpCodes.Call, typeof(DeleteThingPatch).GetMethod(nameof(CheckObject))));
            
            var codes = instructions.ToList();

            int i;
            for (i = 0; i < codes.Count; i++)
            {
                var item = codes[i];

                if (item.operand is MethodInfo method && method.Name == "DeleteThing")
                {
                    codes.InsertRange(i - 3, newInst);

                    break;
                }
            }

            return codes;
        }
    }
    #endregion
}
