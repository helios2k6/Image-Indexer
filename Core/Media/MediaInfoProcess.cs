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

using Core.Environment;
using System;
using System.Diagnostics;
using YAXLib;

namespace Core.Media
{
    /// <summary>
    /// Represents the mediainfo process
    /// </summary>
    public sealed class MediaInfoProcess : IDisposable
    {
        #region private fields
        private static readonly string MEDIAINFO_PROCESS = "mediainfo";

        private readonly Process _process;
        private readonly string _pathToVideoFile;
        private readonly YAXSerializer _serializer;

        private bool _isDisposed;
        private bool _alreadyExecuted;
        #endregion

        #region ctor
        public MediaInfoProcess(string pathToVideoFile)
        {
            _pathToVideoFile = pathToVideoFile;
            _isDisposed = false;
            _alreadyExecuted = false;
            _process = new Process();
            _serializer = new YAXSerializer(typeof(MediaInfo), YAXExceptionHandlingPolicies.DoNotThrow);
        }
        #endregion

        #region public method
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _process.Dispose();
        }

        /// <summary>
        /// Execute this process synchronously and return information about the media file
        /// </summary>
        /// <returns>A MediaInfo object with all of the media file's information</returns>
        /// <remarks>This method is synchronous and will only return once the process is finished</remarks>
        public MediaInfo Execute()
        {
            if (_alreadyExecuted)
            {
                throw new InvalidOperationException("Cannot execute process more than once");
            }

            _alreadyExecuted = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.FileName = EnvironmentTools.CalculateProcessName(MEDIAINFO_PROCESS);
            _process.StartInfo.Arguments = string.Format("--output=XML \"{0}\"", _pathToVideoFile);
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.ErrorDialog = false;

            bool processStarted = _process.Start();
            string mediaInfoOutput = _process.StandardOutput.ReadToEnd();
            if (processStarted == false)
            {
                throw new InvalidOperationException("Could not start process");
            }

            _process.WaitForExit();
            if (_process.ExitCode != 0)
            {
                throw new InvalidOperationException("The MediaInfo process did not execute properly");
            }

            return _serializer.Deserialize(mediaInfoOutput) as MediaInfo;
        }
        #endregion
    }
}