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
using System.Runtime.InteropServices;

namespace Core.Modes
{
    /// <summary>
    /// Changes the error mode of this program
    /// </summary>
    public struct ChangeErrorMode : IDisposable
    {
        /// <summary>
        /// The different error modes
        /// </summary>
        [Flags]
        public enum ErrorModes
        {
            Default = 0x0,
            FailCriticalErrors = 0x1,
            NoGpFaultErrorBox = 0x2,
            NoAlignmentFaultExcept = 0x4,
            NoOpenFileErrorBox = 0x8000
        }

        private int _oldMode;

        /// <summary>
        /// Construct a new ChangeErrorMode struct
        /// </summary>
        /// <param name="mode">The mode to change this program</param>
        public ChangeErrorMode(ErrorModes mode)
        {
            _oldMode = SetErrorMode((int)mode);
        }

        /// <summary>
        /// Change the error mode back to the original error mode of the program
        /// </summary>
        public void Dispose()
        {
            SetErrorMode(_oldMode);
        }

        [DllImport("kernel32.dll")]
        private static extern int SetErrorMode(int newMode);
    }
}
