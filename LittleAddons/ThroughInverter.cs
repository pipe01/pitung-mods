using PiTung;
using PiTung.Components;
using References;
using UnityEngine;

namespace LittleAddons
{
    public class ThroughInverter : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Custom(() =>
                {
                    var obj = GameObject.Instantiate(Prefabs.ThroughBlotter);
                    GameObject.Destroy(obj.GetComponent<Blotter>());

                    var plate = obj.transform.Find("BottomPlate").gameObject;
                    plate.GetComponent<MegaMeshComponent>().MaterialType = MaterialType.CircuitBoard;
                    plate.GetComponent<Renderer>().material.color = new Color(0.6f, 1, 1);

                    return obj;
                });

            ComponentRegistry.CreateNew<ThroughInverter>("throughinverter", "Through Inverter", b);
        }

        protected override void CircuitLogicUpdate()
        {
            Outputs[0].On = !Inputs[0].On;
        }
    }
}
