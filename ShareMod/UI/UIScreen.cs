using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareMod.UI
{
    abstract class UIScreen
    {
        public Remote Remote { get; }
        public bool IsInitialized { get; private set; }
        public bool Visible { get; set; }

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
    }
}
