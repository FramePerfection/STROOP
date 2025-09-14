using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using STROOP.Exceptions;
using STROOP.Structs;

namespace STROOP.Utilities
{
    public class CoreLoop : IDisposable
    {
        private readonly Dictionary<Type, Func<Process, Emulator, IEmuRamIO>> _ioCreationTable = new Dictionary<Type, Func<Process, Emulator, IEmuRamIO>>()
        {
            { typeof(WindowsProcessRamIO),  (p, e) => new WindowsProcessRamIO(p, e) },
            { typeof(DolphinProcessIO),     (p, e) => new DolphinProcessIO(p, e) },
        };

        List<double> _fpsTimes = new List<double>();
        byte[] _ram;
        object _enableLocker = new object();
        object _mStreamProcess = new object();

        public event EventHandler FpsUpdated;

        public bool ShowWarning { get; set; } = false;

        public double FpsInPractice => _fpsTimes.Count == 0 ? 0 : 1 / _fpsTimes.Average();
        public double lastFrameTime => _fpsTimes.Count == 0 ? RefreshRateConfig.RefreshRateInterval : _fpsTimes.Last();

        public void Run()
        {
            ProcessUpdate();
        }

        private void ProcessUpdate()
        {
            Stopwatch frameStopwatch = Stopwatch.StartNew();

            while (!disposedValue)
            {
                frameStopwatch.Restart();
                Application.DoEvents();
                double timeToWait;
                lock (_mStreamProcess)
                {
                    ProcessStream.Instance.RefreshRam();
                    ProcessStream.Instance.OnUpdate?.Invoke();

                    // Calculate delay to match correct FPS
                    frameStopwatch.Stop();
                    double timePassed = (frameStopwatch.ElapsedTicks / (double)Stopwatch.Frequency);
                    timeToWait = RefreshRateConfig.RefreshRateInterval - timePassed;
                    timeToWait = Math.Max(timeToWait, 0);

                    // Calculate Fps
                    while (_fpsTimes.Count() >= 10)
                        _fpsTimes.RemoveAt(0);
                    _fpsTimes.Add(timePassed + timeToWait);
                    FpsUpdated?.Invoke(this, new EventArgs());
                }

                if (timeToWait > 0)
                    Thread.Sleep(new TimeSpan((long)(timeToWait * 10000000)));
                else
                    Thread.Yield();
            }
        }


        public bool OpenSTFile(string fileName)
        {
            StFileIO fileIO = new StFileIO(fileName);
            return ProcessStream.Instance.SwitchIO(fileIO);
        }

        public bool SwitchProcess(Process newProcess, Emulator emulator)
        {
            IEmuRamIO newIo = null;
            try
            {
                newIo = newProcess != null ? _ioCreationTable[emulator.IOType](newProcess, emulator) : null;
                var messages = newIo?.GetLastMessages() ?? string.Empty;
                if (string.Empty != messages)
                    MessageBox.Show(messages, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (DolphinNotRunningGameException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return ProcessStream.Instance.SwitchIO(newIo);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
