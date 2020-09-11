using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstallerLib.Tests
{
    [TestClass]
    public class LauncherTests
    {
        public LauncherTests()
        {
            Launcher = new Launcher(InstallationDirectory);
        }

        public Launcher Launcher { private set; get; }
        private readonly DirectoryInfo InstallationDirectory = new DirectoryInfo(Path.Join(Directory.GetCurrentDirectory(), "test"));

        public void StartProcesses()
        {
            Launcher.Start();
        }
        
        public void StopProcesses()
        {
            Launcher.Stop();
        }

        [TestMethod]
        public async Task CycleProcess()
        {
            StartProcesses();
            await Task.Delay(TimeSpan.FromSeconds(5));
            StopProcesses();
        }
    }
}