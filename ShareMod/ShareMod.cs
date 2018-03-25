using PiTung;
using References;
using ShareMod.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ShareMod
{
    public class ShareMod : Mod
    {
        public override string Name => "ShareMod";
        public override string PackageName => "me.pipe01.ShareMod";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        private Remote Remote = new Remote();

        private IList<UIScreen> Screens = new List<UIScreen>();
        private bool Initialized = false;

        #region Assembly loading
        static ShareMod()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("::: " + args.Name);

            var dic = new Dictionary<string, byte[]>
            {
                ["System.Web"] = Properties.Resources.System_Web,
                ["System.Threading"] = Properties.Resources.System_Threading,
                ["ICSharpCode.SharpZipLib"] = Properties.Resources.ICSharpCode_SharpZipLib,
                ["I18N"] = Properties.Resources.I18N,
                ["I18N.West"] = Properties.Resources.I18N_West
            };

            var name = new AssemblyName(args.Name).Name;

            if (dic.TryGetValue(name, out var b))
            {
                return Assembly.Load(b);
            }
            
            return null;
        }
        #endregion

        public override void AfterPatch()
        {
            Remote.User.TryLoginFromFile();
            
            Screens.Add(new LoginUI(Remote));
            Screens.Add(new StoreUI(Remote));
            Screens.Add(new UploadWorldUI(Remote));
        }

        public override void OnGUI()
        {
            if (!Initialized)
            {
                Initialized = true;
                foreach (var item in Screens)
                {
                    item.Init();
                }
            }

            if (MessageBox.Shown.Count > 0)
            {
                foreach (var item in MessageBox.Shown)
                {
                    item.Draw();
                }
            }
            else
            {
                foreach (var item in Screens)
                {
                    item.Draw();
                }
            }
        }

        public static void PlayButtonSound()
        {
            SoundPlayer.PlaySoundGlobal(Sounds.UIButton);
        }
    }
}
