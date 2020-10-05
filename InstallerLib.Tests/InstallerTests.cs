using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstallerLib.Tests
{
    [TestClass]
    public class InstallerTests
    {
        public InstallerTests()
        {
            InstallationInfo.Current.AppdataDirectory = new DirectoryInfo(Path.Join(Directory.GetCurrentDirectory(), "test"));
            Installer = new Installer();
        }

        private Installer Installer { set; get; }

        [TestMethod]
        public async Task Install()
        {
            var installDir = InstallationInfo.Current.InstallationDirectory;
            var configDir = InstallationInfo.Current.ConfigurationDirectory;

            Console.WriteLine($"Installing binaries to '{installDir}'...");
            await Installer.Install();

            Debug.Assert(installDir.Exists);
            Debug.Assert(configDir.Exists);

            Console.WriteLine($"Installation successful.");
        }

        [TestMethod]
        public void Uninstall()
        {
            var installDir = InstallationInfo.Current.InstallationDirectory;
            
            Console.WriteLine($"Uninstalling binaries in '{installDir}'...");
            Installer.Uninstall();
            
            Debug.Assert(!installDir.Exists);
            
            Console.WriteLine($"Uninstallation successful.");
        }

        [TestMethod]
        public void TestCreateDir()
        {
            var dir = new DirectoryInfo(Path.Join(Environment.GetEnvironmentVariable("HOME"), "test", "balls"));
            dir.Create();
            Debug.Assert(dir.Exists);
        }
    }
}