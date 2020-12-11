using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace InstallerLib.Platform.Windows
{
    public class PowerShellCommand
    {
        public CommandList Commands { get; set; } = new CommandList();
        public bool AsAdmin { get; set; }

        public PowerShellCommand(bool asAdmin = false)
        {
            this.AsAdmin = asAdmin;
        }

        public void Execute(bool wait = false)
        {
            var args = $"-Command {Commands.Aggregate((cmds, cmd) => cmds + "; " + cmd)};";

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

        public class CommandList : IEnumerable<string>
        {
            protected List<string> cmds = new List<string>();

            public static CommandList operator +(CommandList cmdList, string cmd)
            {
                var newList = new CommandList
                {
                    cmds = new List<string>(cmdList.cmds)
                };
                newList.cmds.Add(cmd);
                return newList;
            }

            public static CommandList operator +(CommandList cmdList, IEnumerable<string> cmds)
            {
                var newList = new CommandList
                {
                    cmds = new List<string>(cmdList)
                };
                newList.cmds.AddRange(cmds);
                return newList;
            }

            public IEnumerator<string> GetEnumerator()
            {
                return ((IEnumerable<string>)cmds).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)cmds).GetEnumerator();
            }
        }
    }
}