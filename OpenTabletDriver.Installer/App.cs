using System;
using System.IO;
using System.Reflection;
using Eto.Drawing;
using InstallerLib;

namespace OpenTabletDriver.Installer
{
    public class App
    {
        public App()
        {
            InstallationDirectory = new DirectoryInfo(Path.Join(Directory.GetCurrentDirectory(), "bin"));
            
            Launcher = new Launcher(InstallationDirectory);
            Installer = new AppInstaller(InstallationDirectory);
        }
        
        public static App Current { private set; get; } = new App();

        public DirectoryInfo InstallationDirectory { set; get; }
        public Launcher Launcher { set; get; }
        public AppInstaller Installer { set; get; }

        public Bitmap Logo => _logo.Value;
        private readonly Lazy<Bitmap> _logo = new Lazy<Bitmap>(() => 
        {
            var dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriver.Installer.Assets.otd.png");
            return new Bitmap(dataStream);
        });
    }
}