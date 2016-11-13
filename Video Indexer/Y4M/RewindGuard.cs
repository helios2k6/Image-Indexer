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
using System.IO;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Wraps a stream and rewinds it back to its initial position on disposal, unless 
    /// the "do not rewind" flag is set
    /// </summary>
    internal sealed class RewindGuard : IDisposable
    {
        #region private fields
        private readonly Stream _stream;
        private readonly long _initialPosition;
        private bool _shouldRewind;
        #endregion

        #region ctor
        public RewindGuard(Stream stream)
        {
            _stream = stream;
            _initialPosition = stream.Position;
            _shouldRewind = true;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Signals that this RewindGuard should not rewind on disposal
        /// </summary>
        public void DoNotRewind()
        {
            _shouldRewind = false;
        }

        public void Dispose()
        {
            if (_shouldRewind)
            {
                _stream.Position = _initialPosition;
            }
        }
        #endregion
    }
}
