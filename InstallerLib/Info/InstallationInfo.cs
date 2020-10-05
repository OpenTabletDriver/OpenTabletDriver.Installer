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
            get => this.appdataDirectory ?? new DirectoryInfo(this.defaultAppdataDirectory.Value);
        }
        
        public DirectoryInfo InstallationDirectory
        {
            set => this.installationDirectory = value;
            get => this.installationDirectory ?? new DirectoryInfo(defaultInstallationDirectory);
        }

        public DirectoryInfo ConfigurationDirectory
        {
            set => this.configurationDirectory = value;
            get => this.configurationDirectory ?? new DirectoryInfo(defaultConfigurationDirectory);
        }

        private string defaultInstallationDirectory => Path.Join(AppdataDirectory.FullName, "bin");

        private string defaultConfigurationDirectory => Path.Join(InstallationDirectory.FullName, "Configurations");

        private readonly Lazy<string> defaultAppdataDirectory = new Lazy<string>(() => 
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => Path.Join(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "OpenTabletDriver"),
                RuntimePlatform.Linux   => Path.Join(Environment.GetEnvironmentVariable("HOME"), ".config", "OpenTabletDriver"),
                RuntimePlatform.MacOS   => Path.Join(Environment.GetEnvironmentVariable("HOME"), ""),
                _                       => throw new PlatformNotSupportedException()
            };
        });
    }
}