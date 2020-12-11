namespace InstallerLib.Platform.Windows
{
    public class Shortcut
    {
        public string FullName { get; set; }
        public string Target { get; set; }
        public string WorkingDirectory { get; set; }
        public string Arguments { get; set; }
        public bool Minimized { get; set; } = false;

        public Shortcut(string shortcutFile, string target)
        {
            FullName = shortcutFile;
            Target = target;
        }

        public void Save()
        {
            var cmd = new PowerShellCommand();
            cmd.Commands += new string[]
            {
                "$wShell = New-Object -ComObject WScript.Shell",
                $"$Shortcut = $wShell.CreateShortcut('{FullName}')",
                $"$Shortcut.TargetPath = '{Target}'",
                $"$Shortcut.WindowStyle = {(Minimized ? 7 : 4)}"
            };

            if (!string.IsNullOrEmpty(WorkingDirectory))
                cmd.Commands += $"$Shortcut.WorkingDirectory = '{WorkingDirectory}'";
            if (!string.IsNullOrEmpty(Arguments))
                cmd.Commands += $"$Shortcut.Arguments = '{Arguments}'";

            cmd.Commands += $"$Shortcut.Save()";
            cmd.Execute();
        }

        public static void Save(string FullName, string Target, string WorkingDirectory = null, string Arguments = null, bool Minimized = false)
        {
            var shortcut = new Shortcut(FullName, Target)
            {
                WorkingDirectory = WorkingDirectory,
                Arguments = Arguments,
                Minimized = Minimized
            };
            shortcut.Save();
        }
    }
}