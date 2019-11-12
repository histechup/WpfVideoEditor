using WpfVideoEditor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace WpfVideoEditor.Ffmpeg
{
    /// <summary>
    /// Ffmpeg Interface
    /// </summary>
    public class FfmpegInterface : IDisposable
    {
        public event EventHandler ActiveCountChanged;

        public int ActiveCount { get => ExportProcesses.Count; }

        private readonly List<Process> ExportProcesses = new List<Process>();

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Monitor.Enter(ExportProcesses);
            var active = new List<Process>(ExportProcesses);
            foreach (var p in active)
            {
                p.WaitForExit();
            }
            Monitor.Exit(ExportProcesses);

            if (!disposing)
            {
                throw new InvalidOperationException();
            }
        }

        internal void ExportTrim(FileInfo sourcefile, Clip marking, VideoFormat containerFormat, VideoCodec videoCodec, AudioCodec audioCodec)
        {
            if (!File.Exists("ffmpeg.exe"))
            {
                MessageBox.Show("The ffmpeg program could not be found. It is used to export. Can not export.");
                return;
            }

            var startArguments = new FfmpegArguments(sourcefile, marking.StartMs, marking.EndMs)
            {
                ContainerFormat = containerFormat,
                VideoCodec = videoCodec,
                AudioCodec = audioCodec,
            };
            if (startArguments.TargetFile.Exists)
            {
                var res = MessageBox.Show($"The target file {startArguments.TargetFile} already exists. Do you want to overwrite it?", "Target file already exists", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No)
                {
                    return;
                }
                startArguments.OverwriteTargetFile = true;
            }
            var si = new ProcessStartInfo("ffmpeg.exe", startArguments.ToString())
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                ErrorDialog = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            Debug.WriteLine($"Starting ffmpeg.exe {startArguments}");
            var p = Process.Start(si);
            Monitor.Enter(ExportProcesses);
            ExportProcesses.Add(p);
            Monitor.Exit(ExportProcesses);
            ActiveCountChanged?.Invoke(this, EventArgs.Empty);

            p.EnableRaisingEvents = true;
            p.OutputDataReceived += P_OutputDataReceived;
            p.ErrorDataReceived += P_ErrorDataReceived;
            p.Exited += Process_Exited;

            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e) => Debug.WriteLine($"ffmpeg.exe says: {e.Data}");

        private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e) => Debug.WriteLine($"ffmpeg.exe says ERROR: {e.Data}");

        private void Process_Exited(object sender, EventArgs e)
        {
            var p = (Process)sender;
            Debug.WriteLine($"ffmpeg.exe ended with exit code {p.ExitCode}");
            p.Dispose();
            Monitor.Enter(ExportProcesses);
            ExportProcesses.Remove(p);
            Monitor.Exit(ExportProcesses);
            ActiveCountChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
