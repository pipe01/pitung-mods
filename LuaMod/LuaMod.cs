using LuaMod.Properties;
using PiTung;
using System;
using System.Reflection;

namespace LuaMod
{
    public class LuaMod : Mod
    {
        public override string Name => "LuaMod";
        public override string PackageName => "me.pipe01.LuaMod";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        public override void BeforePatch()
        {
            CpuComponent.Register();
        }


        #region Assembly loading
        static LuaMod()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
#if DEBUG
            Console.WriteLine("::: " + args.Name);
#endif
            
            string name = new AssemblyName(args.Name).Name;
            string resName = name.Replace('.', '_');
            var obj = Resources.ResourceManager.GetObject(resName);

            if (obj != null)
            {
                return Assembly.Load(obj as byte[]);
            }
            
            return null;
        }
        #endregion
    }
}
