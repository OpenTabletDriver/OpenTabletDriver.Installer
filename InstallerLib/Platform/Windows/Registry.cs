using System.Collections.Generic;
using System.Linq;

namespace InstallerLib.Platform.Windows
{
    public class Registry
    {
        public string Key { get; }
        public Dictionary<string, string> Properties;

        public Registry(string registryKey)
        {
            Key = registryKey;
        }

        public void Save()
        {
            var cmd = new PowerShellCommand(true);
            cmd.Commands += $"New-Item {Key}";
            if (Properties.Any())
            {
                cmd.Commands += from property in Properties
                    let cmdUnit = $"Set-ItemProperty -Path '{Key}' -Name '{property.Key}' -Value '{property.Value}'"
                    select cmdUnit;
            }
            cmd.Execute();
        }

        public void Delete()
        {
            var cmd = new PowerShellCommand(true);
            cmd.Commands += $"Remove-Item {Key}";
            cmd.Execute();
        }

        public static void Delete(string registryKey)
        {
            new Registry(registryKey).Delete();
        }
    }
}