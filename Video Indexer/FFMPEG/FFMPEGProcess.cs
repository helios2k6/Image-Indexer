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

namespace VideoIndexer
{
    /// <summary>
    /// Represents the FFMPEG Process
    /// </summary>
    public sealed class FFMPEGProcess : IDisposable
    {
        #region private fields
        private static readonly string FFMPEG_PROC_NAME = "ffmpeg";

        private readonly Process _process;
        private readonly FFMPEGProcessSettings _settings;

        private bool _isDisposed;
        private bool _hasExecuted;
        #endregion

        #region ctor
        /// <summary>
        /// Constructs a new FFMPEG Process object with the provided settings
        /// </summary>
        /// <param name="settings">The process settings</param>
        public FFMPEGProcess(FFMPEGProcessSettings settings)
        {
            _settings = settings;
            _process = new Process();
            _isDisposed = false;
            _hasExecuted = false;
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

            _hasExecuted = true;
            _process.StartInfo.UseShellExecute = true;
            _process.StartInfo.FileName = EnvironmentTools.CalculateProcessName(FFMPEG_PROC_NAME);
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.Arguments = GetArguments();

            var processStarted = _process.Start();
            if (processStarted == false)
            {
                throw new Exception("Unable to start the FFMPEG process");
            }

            _process.WaitForExit();

            if (_process.ExitCode != 0)
            {
                throw new Exception("FFMPEG did not execute properly");
            }
        }

        /// <summary>
        /// Calculate what the timespan string should be when it is embedded in a file name
        /// </summary>
        /// <param name="timespan">The timespan</param>
        /// <returns>A string with the timespan formatted for a file name</returns>
        public static string FormatTimeSpanFileName(TimeSpan timespan)
        {
            return string.Format("{0}_{1}_{2}", timespan.Hours, timespan.Minutes, timespan.Seconds);
        }
        #endregion

        #region private methods
        private string GetArguments()
        {
            return string.Format(
                "-ss {0} -i \"{1}\" -vframes {2} {3} -vf fps={4}/{5} \"{6}\"",
                _settings.StartTime,
                _settings.TargetMediaFile,
                _settings.FramesToOutput,
                GetOutputVideoCodecArgument(),
                _settings.Framerate.Numerator,
                _settings.Framerate.Denominator,
                Path.Combine(_settings.OutputDirectory, GetOutputArgument())
            );
        }

        private string GetOutputVideoCodecArgument()
        {
            switch (_settings.OutputFormat)
            {
                case FFMPEGOutputFormat.PNG:
                    return "-codec:v png";
                case FFMPEGOutputFormat.Y4M:
                    return "-f yuv4mpegpipe -pix_fmt yuv444p";
                default:
                    throw new Exception("Unknown output format");
            }
        }

        private string GetOutputArgument()
        {
            string rootFileName = Path.GetFileNameWithoutExtension(_settings.TargetMediaFile);
            string formattedTimeSpan = FormatTimeSpanFileName(_settings.StartTime);
            switch (_settings.OutputFormat)
            {
                case FFMPEGOutputFormat.PNG:
                    return string.Format(
                        "AUTOGEN_({0})_TIME_({1})_SNAPSHOT_(%02d).png",
                        rootFileName,
                        formattedTimeSpan
                    );
                case FFMPEGOutputFormat.Y4M:
                    return string.Format(
                        "AUTOGEN_({0})_TIME_({1})_RAW_DUMP.y4m",
                        rootFileName,
                        formattedTimeSpan
                    );
                default:
                    throw new Exception("Unknown output format");
            }
        }
        #endregion
    }
}