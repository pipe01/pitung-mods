using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UndoMod
{
    public class Connection
    {
        public enum Ways
        {
            InputInput,
            InputOutput
        }

        public Ways Way { get; set; }
        public GameObject ObjectA { get; set; }
        public GameObject ObjectB { get; set; }

        public Vector3 PositionA { get; }
        public Vector3 PositionB { get; }

        public Connection(Ways way, GameObject objectA, GameObject objectB)
        {
            this.Way = way;
            this.ObjectA = objectA;
            this.ObjectB = objectB;

            this.PositionA = objectA.transform.position;
            this.PositionB = objectB.transform.position;
        }
    }
}
