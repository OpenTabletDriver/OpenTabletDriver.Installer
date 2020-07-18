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

        private static string DaemonName => "OpenTabletDriver.Daemon";
        private static string AppName
        {
            get
            {
                var uxRoot = "OpenTabletDriver.UX.";
                switch (Platform.ActivePlatform)
                {
                    case RuntimePlatform.Windows:
                        return uxRoot + "Wpf";
                    case RuntimePlatform.Linux:
                        return uxRoot + "Gtk";
                    case RuntimePlatform.MacOS:
                        return uxRoot + "MacOS";
                    default:
                        throw new PlatformNotSupportedException();
                }
            }
        }

        public ProcessHandler DaemonProcess { private set; get; }
        public string[] DaemonArgs { private set; get; }
        
        public ProcessHandler AppProcess { private set; get; }
        public string[] AppArgs { private set; get;}

        public DirectoryInfo InstallationDirectory { private set; get; }
        
        private DirectoryInfo _configDir;
        public DirectoryInfo ConfigurationDirectory
        {
            set => _configDir = value;
            get => _configDir ?? new DirectoryInfo(Path.Join(InstallationDirectory.FullName, "Configurations"));
        }

        public void StartDaemon(params string[] args)
        {
            if (DaemonProcess == null || !DaemonProcess.IsRunning)
            {
                var daemonBinPath = Path.Join(
                    InstallationDirectory.FullName,
                    $"{DaemonName}{Platform.ExecutableFileExtension}");
                var daemonBin = new FileInfo(daemonBinPath);
                
                DaemonArgs = new string[]
                {
                    "-c",
                    $"\"{ConfigurationDirectory.FullName}\""
                };

                DaemonProcess = new ProcessHandler(daemonBin)
                {
                    HideWindow = true
                };
                DaemonProcess.Start(DaemonArgs.Union(args).ToArray());
            }
        }

        public void StopDaemon()
        {
            if (DaemonProcess != null || DaemonProcess.IsRunning)
            {
                DaemonProcess.Stop();
                DaemonProcess = null;
            }
        }

        public void StartApp(params string[] args)
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

        public void StopApp()
        {
            if (AppProcess != null || AppProcess.IsRunning)
            {
                AppProcess.Stop();
                AppProcess = null;
            }
        }
    }
}