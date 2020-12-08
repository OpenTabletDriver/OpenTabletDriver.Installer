using System;
using System.Reflection;
using Eto.Drawing;
using InstallerLib;

namespace OpenTabletDriver.Installer
{
    public class App
    {
        public static App Current { private set; get; } = new App();

        public static Bitmap Logo => _logo.Value;

        private static readonly Lazy<Bitmap> _logo = new Lazy<Bitmap>(() => 
        {
            var dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriver.Installer.Assets.otd.png");
            return new Bitmap(dataStream);
        });

        public Launcher Launcher { set; get; } = new Launcher();
        public InstallerLib.Installer Installer { set; get; } = new InstallerLib.Installer();
        public string[] Arguments { set; get; } = { "" };
    }
}