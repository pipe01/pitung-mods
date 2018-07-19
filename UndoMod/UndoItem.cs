using PiTung;
using PiTung.Console;
using References;
using SavedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UndoMod
{
    public abstract class UndoItem
    {
        public abstract void Undo();
    }

    public class WireUndoItem : UndoItem
    {
        public Connection Connection { get; set; }

        public WireUndoItem(Connection connection)
        {
            this.Connection = connection;
        }

        public override void Undo()
        {
            var wireObj = GameObject.Instantiate(Prefabs.Wire);
            Wire wire;
            
            if (Connection.Way == Connection.Ways.InputInput)
                wire = wireObj.AddComponent<InputInputConnection>();
            else
                wire = wireObj.AddComponent<InputOutputConnection>();

            Connection.ObjectA = Connection.ObjectA ?? GetWireReferenceFromPoint(Connection.PositionA);
            Connection.ObjectB = Connection.ObjectB ?? GetWireReferenceFromPoint(Connection.PositionB);

            if (Connection.ObjectA == null || Connection.ObjectB == null)
            {
                SoundPlayer.PlaySoundAt(Sounds.FailDoSomething, wireObj);
                return;
            }

            wire.Point1 = Connection.ObjectA.transform;
            wire.Point2 = Connection.ObjectB.transform;

            wire.SetPegsBasedOnPoints();
            wire.DrawWire();

            StuffConnector.LinkConnection(wire);
            StuffConnector.SetAppropriateConnectionParent(wire);

            wireObj.AddComponent<ObjectInfo>().ComponentType = ComponentType.Wire;

            SoundPlayer.PlaySoundAt(Sounds.ConnectionFinal, wireObj);
        }

        private static GameObject GetWireReferenceFromPoint(Vector3 point)
        {
            foreach (var item in Physics.OverlapSphere(point, 0.1f))
            {
                if (item.GetComponent<CircuitInput>() != null || item.GetComponent<CircuitOutput>() != null)
                {
                    return item.transform.GetChild(0).gameObject;
                }
            }

            return null;
        }
    }

    public class ComponentUndoItem : UndoItem
    {
        public SavedObjectV2 SavedObject { get; set; }
        public Transform Parent { get; set; }
        public List<Wire> Wires { get; set; } = new List<Wire>();

        public override void Undo()
        {
            IGConsole.Log("Component SavedObject is null: " + (SavedObject == null));

            SavedObjectUtilities.LoadSavedObject(SavedObject, Parent);
            
            //foreach (var item in Wires)
            //{
            //    if (item is InputInputConnection ii)
            //    {
            //        StuffConnector.LinkInputs(ii);
            //    }
            //    else if (item is InputOutputConnection io)
            //    {
            //        StuffConnector.LinkInputOutput(io);
            //    }
            //}
        }
    }
}
