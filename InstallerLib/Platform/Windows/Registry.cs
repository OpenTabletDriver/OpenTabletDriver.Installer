using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace InstallerLib.Platform.Windows
{
    public class Registry
    {
        public string Key { get; }
        public Dictionary<string, string> Values;
        public bool IsUserEditable => userEditable(Key);

        public Registry(string registryKey)
        {
            Key = registryKey;
        }

        public void Save()
        {
            var cmd = new PowerShellCommand();
            cmd.AddCommands($"New-Item {Key}");
            if (Values.Any())
            {
                cmd.AddCommands(from property in Values
                    let cmdUnit = $"Set-ItemProperty -Path '{Key}' -Name '{property.Key}' -Value '{property.Value}'"
                    select cmdUnit);
            }
            cmd.Execute();
        }

        public static void Delete(string registryKey)
        {
            var isUserEditable = userEditable(registryKey);
            var cmd = new PowerShellCommand(!isUserEditable);
            cmd.AddCommands($"Remove-Item {registryKey}");
            cmd.Execute();
        }

        public void Delete()
        {
            Delete(Key);
        }

        private static bool userEditable(string key)
        {
            return Regex.IsMatch(key, "^HKCU");
        }
    }
}