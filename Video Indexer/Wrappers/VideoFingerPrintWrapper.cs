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
using System.Linq;

namespace VideoIndexer.Wrappers
{
    /// <summary>
    /// The fingerprints for a video
    /// </summary>
    public sealed class VideoFingerPrintWrapper : IEquatable<VideoFingerPrintWrapper>
    {
        #region public properties
        /// <summary>
        /// The file path to the video
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The fingerprints for all of the frames of this video
        /// </summary>
        public FrameFingerPrintWrapper[] FingerPrints { get; set; }

        /// <summary>
        /// Gets the size of this video fingerprint wrapper in bytes
        /// </summary>
        public ulong MemorySize
        {
            get { return (ulong)FingerPrints.LongLength * (96ul); }
        }
        #endregion

        #region public methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(VideoFingerPrintWrapper other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return string.Equals(FilePath, other.FilePath, StringComparison.Ordinal) &&
                Enumerable.SequenceEqual(FingerPrints, other.FingerPrints);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (EqualsPreamble(obj) == false)
            {
                return false;
            }

            return Equals(obj as VideoFingerPrintWrapper);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int fingerPrintHashCode = FingerPrints != null
                ? FingerPrints.Aggregate(0, (acc, f) => acc + f.GetHashCode())
                : 0;

            return FilePath.GetHashCode() ^ fingerPrintHashCode;
        }
        #endregion

        #region private methods
        private bool EqualsPreamble(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return true;
        }
        #endregion
    }
}
