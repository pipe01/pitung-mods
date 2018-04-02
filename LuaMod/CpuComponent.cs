using MoonSharp.Interpreter;
using PiTung.Components;
using PiTung.Mod_utilities;
using UnityEngine;

namespace LuaMod
{
    internal class CpuComponent : UpdateHandler
    {
        public static void Register()
        {
            var build = PrefabBuilder.Cube
                .WithSide(CubeSide.Top, SideType.Input)
                .WithSide(CubeSide.Front, SideType.Input)
                .WithSide(CubeSide.Back, SideType.Output)
                .WithColor(new Color(0, 1, 0));

            ComponentRegistry.CreateNew<CpuComponent>(this, "cpu", "CPU", build);
        }

        private static readonly string DefaultScript = @"function update()

end";

        private string _luaScript = DefaultScript;
        private Script Script = new Script();
        private DynValue UpdateFunc;
        private Hologram ErrorHologram;

        public string CurrentError = "";

        [SaveThis]
        public string LuaScript
        {
            get => _luaScript;
            set
            {
                _luaScript = value;

                try
                {
                    this.Script.LoadString(value);
                    this.UpdateFunc = this.Script.Globals["update"] as DynValue;
                }
                catch (SyntaxErrorException ex)
                {
                    this.ErrorHologram.Visible = true;
                    this.CurrentError = ex.Message;
                }
            }
        }

        protected override void OnAwake()
        {
            this.gameObject.AddComponent<ScriptInteract>();
            this.ErrorHologram = new Hologram("Error!", this.gameObject)
            {
                Visible = false
            };
        }

        protected override void CircuitLogicUpdate()
        {
            if (this.UpdateFunc == null)
                return;

            this.UpdateFunc.Function.Call();
        }
    }
}
