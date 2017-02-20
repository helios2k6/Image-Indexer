/* 
 * Copyright (c) 2015 Andrew Johnson
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
 * Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Core.Environment;

namespace FFMPEGWrapper
{
    /// <summary>
    /// Represents the FFMPEG Process
    /// </summary>
    public sealed class FFMPEGProcess : IDisposable
    {
        #region public static fields
        public static readonly int DefaultBufferSize = 64000;
        #endregion

        #region private fields
        private static readonly string FFMPEGProcName = "ffmpeg";

        private readonly Process _process;
        private readonly FFMPEGProcessVideoSettings _settings;
        private readonly CancellationToken _cancellationToken;
        private readonly MemoryStream _stdErrorStream;
        private readonly Action<byte[], int> _byteChunkCallback;

        private bool _isDisposed;
        private bool _hasExecuted;
        #endregion

        #region ctor
        /// <summary>
        /// Constructs a new FFMPEG Process object with the provided settings
        /// </summary>
        /// <param name="settings">The process settings</param>
        public FFMPEGProcess(
            FFMPEGProcessVideoSettings settings,
            CancellationToken cancellationToken,
            Action<byte[], int> byteChunkCallback
        )
        {
            _settings = settings;
            _cancellationToken = cancellationToken;
            _process = new Process();
            _isDisposed = false;
            _hasExecuted = false;
            _stdErrorStream = new MemoryStream();
            _byteChunkCallback = byteChunkCallback;

            _cancellationToken.Register(KillProcess);
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets the standard error stream
        /// </summary>
        public Stream StandardErrorStream
        {
            get { return _stdErrorStream; }
        }
        #endregion

        #region public methods
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _process.Dispose();
            _stdErrorStream.Dispose();
        }

        /// <summary>Execute the FFMPEG executable</summary>
        /// <remarks>
        /// This process can only be executed once. If it has already
        /// been executed, an exception will be thrown.
        /// </remarks>
        public void Execute()
        {
            if (_hasExecuted)
            {
                throw new InvalidOperationException("This process has already executed");
            }

            if (_isDisposed)
            {
                throw new ObjectDisposedException("this");
            }

            try
            {
                // Note: http://stackoverflow.com/questions/15922175/ffmpeg-run-from-shell-runs-properly-but-does-not-when-called-from-within-net
                _hasExecuted = true;
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.RedirectStandardInput = true;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.FileName = EnvironmentTools.CalculateProcessName(FFMPEGProcName);
                _process.StartInfo.Arguments = GetArguments();
                _process.StartInfo.ErrorDialog = false;

                var processStarted = _process.Start();
                if (processStarted == false)
                {
                    throw new Exception("Unable to start the FFMPEG process");
                }

                Task stderr = Task.Factory.StartNew(() =>
                {
                    using (StreamWriter writer = new StreamWriter(_stdErrorStream))
                    {
                        while (_process.StandardError.EndOfStream == false)
                        {
                            writer.WriteLine(_process.StandardError.ReadLine());
                        }
                    }
                });

                int bytesRead = 0;
                byte[] stdoutBuffer = new byte[DefaultBufferSize];
                using (var binaryReader = new BinaryReader(_process.StandardOutput.BaseStream))
                {
                    do
                    {
                        bytesRead = binaryReader.Read(stdoutBuffer, 0, DefaultBufferSize);
                        if (bytesRead > 0)
                        {
                            _byteChunkCallback.Invoke(stdoutBuffer, bytesRead);
                        }
                    } while (bytesRead > 0);
                }

                stderr.Wait(_cancellationToken);
                _process.WaitForExit();
            }
            finally
            {
                _stdErrorStream.Close();
            }

            if (_process.ExitCode != 0 && _cancellationToken.IsCancellationRequested == false)
            {
                throw new Exception("FFMPEG did not execute properly");
            }
        }
        #endregion

        #region private methods
        private void KillProcess()
        {
            _process.Kill();
        }

        private string GetArguments()
        {
            return string.Format(
                "-i \"{0}\" -f rawvideo -pix_fmt bgr24 -vf fps={1}/{2} -",
                _settings.TargetMediaFile,
                _settings.FrameRateNumerator,
                _settings.FrameRateDenominator
            );
        }

        private string GetFilters()
        {
            switch (_settings.Mode)
            {
                case FFMPEGMode.PlaybackAtFourX:
                    return string.Empty;
                case FFMPEGMode.SeekFrame:
                    return string.Format(@"-vf ""select = eq(n\, 100)"" -vframes 1");
                default:
                    throw new InvalidOperationException("Unknown mode");
            }
        }
        #endregion
    }
}