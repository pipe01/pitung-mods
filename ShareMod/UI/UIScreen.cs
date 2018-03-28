using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareMod.UI
{
    abstract class UIScreen
    {
        private static bool IsMainMenuPlaying = true;
        private static IDictionary<UIScreen, bool> ScreensWant = new Dictionary<UIScreen, bool>();
        
        public Remote Remote { get; }
        public bool IsInitialized { get; private set; }
        public bool Visible { get; set; }

        public virtual bool RequireMainMenu { get; }

        public UIScreen(Remote remote)
        {
            this.Remote = remote;
        }

        public abstract void Draw();
        public virtual void Init()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void Show()
        {
            Visible = true;
            PauseMainMenuBackground();
        }

        public void Hide()
        {
            Visible = false;
            ResumeMainMenuBackground();
        }

        protected void SetMainMenuPlaying(bool playing)
        {
            if (playing)
                ResumeMainMenuBackground();
            else
                PauseMainMenuBackground();
        }

        private void UpdateMainMenuBackground()
        {
            if (ScreensWant.Any(o => !o.Value))
            {
                if (IsMainMenuPlaying)
                {
                    IsMainMenuPlaying = false;
                    RunMainMenu.Instance.CameraMovementDirector.Pause();
                }
            }
            else
            {
                if (!IsMainMenuPlaying)
                {
                    IsMainMenuPlaying = true;
                    RunMainMenu.Instance.CameraMovementDirector.Resume();
                }
            }
        }

        protected void PauseMainMenuBackground()
        {
            ScreensWant[this] = false;
            UpdateMainMenuBackground();
        }

        protected void ResumeMainMenuBackground()
        {
            ScreensWant[this] = true;
            UpdateMainMenuBackground();
        }
    }
}
