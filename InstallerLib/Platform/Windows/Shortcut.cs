namespace InstallerLib.Platform.Windows
{
    public class Shortcut
    {
        public string FullName { get; set; }
        public string Target { get; set; }
        public string WorkingDirectory { get; set; }
        public string Arguments { get; set; }
        public bool Minimized { get; set; }

        public Shortcut(string shortcutFile, string target)
        {
            FullName = shortcutFile;
            Target = target;
        }

        public void Save()
        {
            var cmd = new PowerShellCommand();
            cmd.AddCommands(
                "$wShell = New-Object -ComObject WScript.Shell",
                $"$Shortcut = $wShell.CreateShortcut('{FullName}')",
                $"$Shortcut.TargetPath = '{Target}'",
                $"$Shortcut.WindowStyle = {(Minimized ? 7 : 4)}"
            );

            if (!string.IsNullOrEmpty(WorkingDirectory))
                cmd.AddCommands($"$Shortcut.WorkingDirectory = '{WorkingDirectory}'");
            if (!string.IsNullOrEmpty(Arguments))
                cmd.AddCommands($"$Shortcut.Arguments = '{Arguments}'");

            cmd.AddCommands($"$Shortcut.Save()");
            cmd.Execute();
        }

        public static void Create(string FullName, string Target, string WorkingDirectory = null, string Arguments = null, bool Minimized = false)
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