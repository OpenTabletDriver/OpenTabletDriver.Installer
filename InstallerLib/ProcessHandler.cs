using System;
using System.Diagnostics;
using System.IO;

namespace InstallerLib
{
    public class ProcessHandler
    {
        public ProcessHandler(FileInfo binFile)
        {
            BinaryFile = binFile;
        }

        private FileInfo BinaryFile { set; get; }
        private Process Process { set; get; }
        public bool IsRunning { private set; get; }
        public bool HideWindow { set; get; }
        public event EventHandler<int> Exited;

        public void Start(params string[] args)
        {
            if (!BinaryFile.Exists)
                throw new FileNotFoundException("The binary file does not exist.", BinaryFile.FullName);

            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = BinaryFile.FullName,
                    Arguments = string.Join(' ', args),
                    CreateNoWindow = HideWindow,
                    WorkingDirectory = BinaryFile.Directory.FullName
                }
            };
            Process.Exited += (s, e) =>
            {
                Exited?.Invoke(this, Process.ExitCode);
                IsRunning = false;
            };
            Process.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            Process.Kill();
        }
    }
}