using System;
using System.Collections.Generic;
using System.IO;

namespace InstallerLib
{
    public class Launcher
    {
        public Launcher(DirectoryInfo installationDirectory)
        {
            InstallationDirectory = installationDirectory;
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

        private ProcessHandler DaemonProcess { set; get; }
        private ProcessHandler AppProcess { set; get; }

        public DirectoryInfo InstallationDirectory { private set; get; }

        public void StartDaemon(params string[] args)
        {
            if (DaemonProcess == null)
            {
                var daemonBinPath = Path.Join(
                    InstallationDirectory.FullName,
                    $"{DaemonName}/{DaemonName}{Platform.ExecutableFileExtension}");
                var daemonBin = new FileInfo(daemonBinPath);
                DaemonProcess = new ProcessHandler(daemonBin);
                DaemonProcess.Start(args);
            }
        }

        public void StopDaemon()
        {
            if (DaemonProcess != null)
                DaemonProcess.Stop();
        }

        public void StartFrontend(params string[] args)
        {
            if (AppProcess == null)
            {
                var appBinPath = Path.Join(
                    InstallationDirectory.FullName,
                    $"{AppName}/{AppName}{Platform.ExecutableFileExtension}");
                var appBin = new FileInfo(appBinPath);
                AppProcess = new ProcessHandler(appBin);
                AppProcess.Start(args);
            }
        }

        public void StopFrontend()
        {
            if (AppProcess != null)
                AppProcess.Stop();
        }
    }
}