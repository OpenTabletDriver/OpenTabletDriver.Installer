using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InstallerLib
{
    public class Launcher
    {
        public Launcher(DirectoryInfo installationDirectory, DirectoryInfo configurationDirectory = null)
        {
            InstallationDirectory = installationDirectory;
            ConfigurationDirectory = configurationDirectory;
            
            AppArgs = new string[0];
        }

        internal const string DaemonName = "OpenTabletDriver.Daemon";
        internal const string ConsoleName = "OpenTabletDriver.Console";
        private const string UXPrefix = "OpenTabletDriver.UX.";
        internal static string AppName => Platform.ActivePlatform switch
        {
            RuntimePlatform.Windows => UXPrefix + "Wpf",
            RuntimePlatform.Linux   => UXPrefix + "Gtk",
            RuntimePlatform.MacOS   => UXPrefix + "MacOS",
            _                       => throw new PlatformNotSupportedException()
        };

        public ProcessHandler AppProcess { private set; get; }
        public string[] AppArgs { private set; get;}

        public DirectoryInfo InstallationDirectory { private set; get; }
        
        private DirectoryInfo _configDir;
        public DirectoryInfo ConfigurationDirectory
        {
            set => _configDir = value;
            get => _configDir ?? new DirectoryInfo(Path.Join(InstallationDirectory.FullName, "Configurations"));
        }

        public void Start(params string[] args)
        {
            if (AppProcess == null || !AppProcess.IsRunning)
            {
                var appBinPath = Path.Join(
                    InstallationDirectory.FullName,
                    $"{AppName}{Platform.ExecutableFileExtension}");
                var appBin = new FileInfo(appBinPath);

                AppArgs = new string[0];

                AppProcess = new ProcessHandler(appBin);
                AppProcess.Start(AppArgs.Union(args).ToArray());
            }
        }

        public void Stop()
        {
            if (AppProcess != null || AppProcess.IsRunning)
            {
                AppProcess.Stop();
                AppProcess = null;
            }
        }
    }
}