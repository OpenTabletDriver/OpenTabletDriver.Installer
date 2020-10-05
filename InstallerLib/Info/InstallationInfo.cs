using System;
using System.IO;
using InstallerLib.Info;

namespace InstallerLib
{
    public class InstallationInfo
    {
        public static InstallationInfo Current { private set; get; } = new InstallationInfo();
        
        private DirectoryInfo appdataDirectory, installationDirectory, configurationDirectory;

        public DirectoryInfo AppdataDirectory
        {
            set => this.appdataDirectory = value;
            get => this.appdataDirectory ?? new DirectoryInfo(this.defaultAppdataDirectory);
        }
        
        public DirectoryInfo InstallationDirectory
        {
            set => this.installationDirectory = value;
            get => this.installationDirectory ?? new DirectoryInfo(this.defaultInstallationDirectory);
        }

        public DirectoryInfo ConfigurationDirectory
        {
            set => this.configurationDirectory = value;
            get => this.configurationDirectory ?? new DirectoryInfo(this.defaultConfigurationDirectory);
        }

        private string defaultInstallationDirectory => Path.Join(AppdataDirectory.FullName, "bin");

        private string defaultConfigurationDirectory => Path.Join(InstallationDirectory.FullName, "Configurations");

        private string defaultAppdataDirectory => SystemInfo.CurrentPlatform switch
        {
            RuntimePlatform.Windows => Path.Join(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "OpenTabletDriver"),
            RuntimePlatform.Linux   => Path.Join(Environment.GetEnvironmentVariable("HOME"), ".config", "OpenTabletDriver"),
            RuntimePlatform.MacOS   => Path.Join(Environment.GetEnvironmentVariable("HOME"), "Library", "Application Support", "OpenTabletDriver"),
            _                       => throw new PlatformNotSupportedException()
        };
    }
}