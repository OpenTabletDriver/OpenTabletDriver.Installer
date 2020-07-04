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
            Installer = new Installer(InstallationDirectory);
        }

        private Installer Installer { set; get; }
        private readonly DirectoryInfo InstallationDirectory = new DirectoryInfo(Path.Join(Directory.GetCurrentDirectory(), "test"));

        [TestMethod]
        public async Task Install()
        {
            Console.WriteLine($"Installing binaries to '{InstallationDirectory}'...");
            await Installer.InstallBinaries();
            Debug.Assert(InstallationDirectory.Exists);
            Console.WriteLine($"Installation successful.");
        }

        [TestMethod]
        public async Task Uninstall()
        {
            Console.WriteLine($"Uninstalling binaries in '{InstallationDirectory}'...");
            await Installer.DeleteBinaries();
            Debug.Assert(!InstallationDirectory.Exists);
            Console.WriteLine($"Uninstallation successful.");
        }
    }
}