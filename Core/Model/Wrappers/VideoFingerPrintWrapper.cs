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

namespace Core.Model.Wrappers
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
                CompareFingerPrint(other.FingerPrints);
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
        private bool CompareFingerPrint(FrameFingerPrintWrapper[] other)
        {
            if (FingerPrints == other)
            {
                return true;
            }

            if (FingerPrints == null && other != null)
            {
                return false;
            }

            if (FingerPrints != null && other == null)
            {
                return false;
            }

            if (FingerPrints.Length != other.Length)
            {
                return false;
            }

            FrameFingerPrintWrapper[] sortedFingerPrints = new FrameFingerPrintWrapper[FingerPrints.Length];
            FrameFingerPrintWrapper[] otherSortedFingerPrints = new FrameFingerPrintWrapper[FingerPrints.Length];

            Array.Copy(FingerPrints, 0, sortedFingerPrints, 0, sortedFingerPrints.Length);
            Array.Copy(other, 0, otherSortedFingerPrints, 0, otherSortedFingerPrints.Length);

            Array.Sort(sortedFingerPrints, (a, b) => a.FrameNumber - b.FrameNumber);
            Array.Sort(otherSortedFingerPrints, (a, b) => a.FrameNumber - b.FrameNumber);

            for (int i = 0; i < FingerPrints.Length; i++)
            {
                if (Equals(sortedFingerPrints[i], otherSortedFingerPrints[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }

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
