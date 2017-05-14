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
using System.Text;

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
        private readonly Action<byte[], int> _byteChunkCallback;
        private readonly StringBuilder _stdErrorStream;
        private bool _shouldThrowOnErrorCode;

        private bool _isDisposed;
        private bool _hasStartedExecuting;
        private bool _hasFinishedExecuting;
        #endregion

        #region ctor
        /// <summary>
        /// Constructs a new FFMPEG Process object with the provided settings
        /// </summary>
        /// <param name="settings">The process settings</param>
        public FFMPEGProcess(
            FFMPEGProcessVideoSettings settings,
            Action<byte[], int> byteChunkCallback
        ) : this(settings, byteChunkCallback, true)
        {
        }

        /// <summary>
        /// Constructs a new FFMPEG Process object with the provided settings
        /// </summary>
        /// <param name="settings">The process settings</param>
        /// <param name="byteChunkCallback">The callback to put the raw bytes from ffmpeg</param>
        /// <param name="cancellationToken">The cancellation token to listen to</param>
        /// <param name="shouldThrowOnErrorCode">Whether this should throw when the ffmpeg process exits with an error code</param>
        public FFMPEGProcess(
            FFMPEGProcessVideoSettings settings,
            Action<byte[], int> byteChunkCallback,
            bool shouldThrowOnErrorCode
        )
        {
            _settings = settings;
            _process = new Process();
            _isDisposed = false;
            _hasStartedExecuting = false;
            _hasFinishedExecuting = false;
            _stdErrorStream = new StringBuilder();
            _byteChunkCallback = byteChunkCallback;
            _shouldThrowOnErrorCode = shouldThrowOnErrorCode;
        }
        #endregion

        #region public methods
        public int GetErrorCode()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot get error code from disposed object");
            }

            if (_hasStartedExecuting == false)
            {
                throw new InvalidOperationException("Cannot get error code from process that has not started");
            }

            if (_hasFinishedExecuting == false)
            {
                throw new InvalidOperationException("Cannot get error code from unfinished ffmpeg process");
            }

            return _process.ExitCode;
        }

        public string GetErrorStreamMessage()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot get message from disposed object");
            }

            if (_hasStartedExecuting == false)
            {
                throw new InvalidOperationException("Cannot get message from process that has not started");
            }

            if (_hasFinishedExecuting == false)
            {
                throw new InvalidOperationException("Cannot get message from unfinished ffmpeg process");
            }

            return _stdErrorStream.ToString();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _process.Dispose();
        }

        /// <summary>Execute the FFMPEG executable</summary>
        /// <remarks>
        /// This process can only be executed once. If it has already
        /// been executed, an exception will be thrown.
        /// </remarks>
        public void Execute()
        {
            if (_hasStartedExecuting)
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
                _hasStartedExecuting = true;
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
                    while (_process.StandardError.EndOfStream == false)
                    {
                        _stdErrorStream.AppendLine(_process.StandardError.ReadLine());
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

                stderr.Wait();
                _process.WaitForExit();
            }
            finally
            {
                _hasFinishedExecuting = true;
            }

            if (_shouldThrowOnErrorCode && _process.ExitCode != 0)
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
                "-i \"{0}\" -f rawvideo -pix_fmt bgr24 -vf {1} -",
                _settings.TargetMediaFile,
                GetFilters()
            );
        }

        private string GetFilters()
        {
            var filterString = new StringBuilder();
            filterString.Append(string.Format(@"""fps={0}/{1}", _settings.FrameRateNumerator, _settings.FrameRateDenominator));
            switch (_settings.Mode)
            {
                case FFMPEGMode.PlaybackAtFourX:
                    return filterString.Append("\"").ToString();
                case FFMPEGMode.SeekFrame:
                    return filterString.Append(string.Format(@", select=eq(n\,{0})"" -vframes 1", _settings.TargetFrame)).ToString();
                default:
                    throw new InvalidOperationException("Unknown mode");
            }
        }
        #endregion
    }
}