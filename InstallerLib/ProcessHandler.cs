using System;
using System.Collections.Generic;
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

        public FileInfo BinaryFile { private set; get; }
        public Process Process { private set; get; }
        public bool IsRunning { private set; get; }
        public bool HideWindow { set; get; } = false;
        public event EventHandler<int> Exited;

        public Dictionary<DateTime, string> Log { private set; get; } = new Dictionary<DateTime, string>();

        public void Start(params string[] args)
        {
            if (!BinaryFile.Exists)
                throw new FileNotFoundException("The binary file does not exist.", BinaryFile.FullName);

            Log.Clear();
            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = BinaryFile.FullName,
                    Arguments = string.Join(' ', args),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = HideWindow
                }
            };
            Process.OutputDataReceived += (s, e) => Log.Add(DateTime.Now, e?.Data);
            Process.ErrorDataReceived += (s, e) => Log.Add(DateTime.Now, e?.Data);
            Process.Exited += (s, e) => 
            {
                Exited?.Invoke(this, Process.ExitCode);
                IsRunning = false;
            };
            Process.Start();
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
            IsRunning = true;
        }

        public void Stop()
        {
            Process.Kill();
        }
    }
}