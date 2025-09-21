using STROOP.Core;
using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using STROOP.Exceptions;
using STROOP.Structs;
using System.Reflection;

namespace STROOP.Utilities
{
    public class CoreLoop
    {
        List<double> _fpsTimes = new List<double>();
        byte[] _ram;
        object _enableLocker = new object();
        object _mStreamProcess = new object();

        public event EventHandler FpsUpdated;

        public bool ShowWarning { get; set; } = false;

        public double FpsInPractice => _fpsTimes.Count == 0 ? 0 : 1 / _fpsTimes.Average();
        public double lastFrameTime => _fpsTimes.Count == 0 ? RefreshRateConfig.RefreshRateInterval : _fpsTimes.Last();

        public void Run(CancellationToken cancellationToken)
        {
            Stopwatch frameStopwatch = Stopwatch.StartNew();

            while (!cancellationToken.IsCancellationRequested)
            {
                double timeToWait;
                lock (_mStreamProcess)
                {
                    ProcessStream.Instance.RefreshRam();
                    ProcessStream.Instance.OnUpdate?.Invoke();
                }

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

                frameStopwatch.Restart();
                Application.DoEvents();

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
                newIo = newProcess != null
                    ? (IEmuRamIO)Activator.CreateInstance(
                        emulator.IOType,
                        BindingFlags.Default,
                        null,
                        [newProcess, emulator],
                        null
                    )
                    : null;
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
    }
}
