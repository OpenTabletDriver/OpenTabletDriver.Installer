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
            Launcher.StartDaemon();
            Launcher.StartApp();
        }
        
        public void StopProcesses()
        {
            Launcher.StopApp();
            Launcher.StopDaemon();
            
            foreach (var message in Launcher.DaemonProcess.Log)
                Console.WriteLine(message.Value);
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