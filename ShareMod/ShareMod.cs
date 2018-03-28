using PiTung;
using PiTung.Console;
using References;
using ShareMod.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ShareMod
{
    public class ShareMod : Mod
    {
        public override string Name => "ShareMod";
        public override string PackageName => "me.pipe01.ShareMod";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.1");
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        private Remote Remote = new Remote();

        private IList<UIScreen> Screens = new List<UIScreen>();
        private bool Initialized = false, HiddenCanvases = false;
        private IList<Canvas> WereVisible = new List<Canvas>();

        #region Assembly loading
        static ShareMod()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
#if DEBUG
            Console.WriteLine("::: " + args.Name);
#endif

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

            AddUI<LoginUI>();
            AddUI<AccountUI>();
            AddUI<StoreUI>();
            AddUI<UploadWorldUI>();
            
            void AddUI<T>() where T : UIScreen
            {
                Screens.Add(Activator.CreateInstance(typeof(T), Remote) as T);
            }
        }

        public override void OnGUI()
        {
            if (!ModUtilities.IsOnMainMenu)
                return;

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
                SetGameUIVisible(false);

                foreach (var item in MessageBox.Shown)
                {
                    item.Draw();
                }
            }
            else
            {
                SetGameUIVisible(true);

                foreach (var item in Screens)
                {
                    if (item.RequireMainMenu && !ModUtilities.IsOnMainMenu)
                        continue;

                    item.Draw();
                }
            }
        }
        
        public void SetGameUIVisible(bool visible)
        {
            if (HiddenCanvases == visible)
                return;
            HiddenCanvases = visible;
            
            var i = RunMainMenu.Instance;

            SetVisible(i.AboutCanvas, visible);
            SetVisible(i.DeleteGameCanvas, visible);
            SetVisible(i.LoadGameCanvas, visible);
            SetVisible(i.MainMenuCanvas, visible);
            SetVisible(i.NewGameCanvas, visible);
            SetVisible(i.OptionsCanvas, visible);
            SetVisible(i.RenameGameCanvas, visible);

            void SetVisible(Canvas v, bool vis)
            {
                if (v.enabled && !vis)
                {
                    WereVisible.Add(v);
                    v.enabled = false;
                }
                else if (vis && WereVisible.Contains(v))
                {
                    WereVisible.Remove(v);
                    v.enabled = true;
                }
            }
        }

        public static void PlayButtonSound()
        {
            SoundPlayer.PlaySoundGlobal(Sounds.UIButton);
        }
    }
}
