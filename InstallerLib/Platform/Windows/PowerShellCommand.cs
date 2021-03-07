using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace InstallerLib.Platform.Windows
{
    public class PowerShellCommand
    {
        private readonly List<string> commands = new List<string>();
        public bool AsAdmin { get; set; }

        public PowerShellCommand(bool asAdmin = false)
        {
            this.AsAdmin = asAdmin;
        }

        public void AddCommands(params string[] commands)
        {
            this.commands.AddRange(commands);
        }

        public void AddCommands(IEnumerable<string> commands)
        {
            this.commands.AddRange(commands);
        }

        public void Execute(bool wait = false)
        {
            var args = $"-Command {commands.Aggregate((cmds, cmd) => cmds + "; " + cmd)};";

            var powershell = new Process()
            {
                StartInfo = new ProcessStartInfo("powershell", args)
                {
                    UseShellExecute = AsAdmin,
                    Verb = AsAdmin ? "runas" : string.Empty,
                    CreateNoWindow = !AsAdmin,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            try
            {
                powershell.Start();
                if (wait)
                    powershell.WaitForExit();
            }
            catch (Win32Exception e) when (e.NativeErrorCode == 1223)
            {
                throw new OperationCanceledException();
            }
        }
    }
}