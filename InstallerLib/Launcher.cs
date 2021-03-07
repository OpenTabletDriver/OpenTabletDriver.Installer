using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using InstallerLib.Info;

namespace InstallerLib
{
    public class Launcher
    {
        public Launcher()
        {
            AppArgs = Array.Empty<string>();
        }

        internal const string DaemonName = "OpenTabletDriver.Daemon";
        internal const string ConsoleName = "OpenTabletDriver.Console";
        private const string UXPrefix = "OpenTabletDriver.UX.";
        internal static string AppName => SystemInfo.CurrentPlatform switch
        {
            RuntimePlatform.Windows => UXPrefix + "Wpf",
            RuntimePlatform.Linux   => UXPrefix + "Gtk",
            RuntimePlatform.MacOS   => UXPrefix + "MacOS",
            _                       => throw new PlatformNotSupportedException()
        };

        public ProcessHandler AppProcess { private set; get; }
        public string[] AppArgs { private set; get;}

        public void Start(params string[] args)
        {
            if (AppProcess == null || !AppProcess.IsRunning)
            {
                var appBinPath = Path.Join(
                    InstallationInfo.Current.InstallationDirectory.FullName,
                    $"{AppName}{SystemInfo.ExecutableFileExtension}");
                var appBin = new FileInfo(appBinPath);

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