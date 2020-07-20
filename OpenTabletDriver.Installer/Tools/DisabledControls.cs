using System;
using Eto.Forms;

namespace OpenTabletDriver.Installer.Tools
{
    public class DisabledControls : IDisposable
    {
        private Control[] controls;
        
        public DisabledControls(params Control[] controls)
        {
            this.controls = controls;
            foreach (var control in this.controls)
                control.Enabled = false;
        }

        public void Dispose()
        {
            foreach (var control in controls)
                control.Enabled = true;
        }
    }
}