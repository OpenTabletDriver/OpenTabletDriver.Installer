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
        public event EventHandler<int> Exited;

        public Dictionary<DateTime, string> Log { private set; get; } = new Dictionary<DateTime, string>();

        public void Start(params string[] args)
        {
            Log.Clear();
            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = string.Join(' ', args),                    
                }
            };
            Process.OutputDataReceived += (s, e) => Log.Add(DateTime.Now, e.Data);
            Process.ErrorDataReceived += (s, e) => Log.Add(DateTime.Now, e.Data);
            Process.Exited += (s, e) => Exited?.Invoke(this, Process.ExitCode);
        }

        public void Stop()
        {
            Process.Close();
        }
    }
}