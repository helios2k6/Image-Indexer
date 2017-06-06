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

using Core.DSA;
using System;
using System.Linq;

namespace Core.Model.Wrappers
{
    /// <summary>
    /// A fingerprint of a photo file
    /// </summary>
    public sealed class PhotoFingerPrintWrapper :
        IEquatable<PhotoFingerPrintWrapper>,
        IMetric<FrameFingerPrintWrapper>,
        IMetric<PhotoFingerPrintWrapper>
    {
        #region public properties
        /// <summary>
        /// the path to the file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The hashcode of this photo
        /// </summary>
        public ulong PHash { get; set; }

        /// <summary>
        /// The gray-scale thumbnail of the image
        /// </summary>
        public byte[] EdgeGrayScaleThumb { get; set; }
        #endregion

        #region public methods
        /// <summary>
        /// Calculate the distance from this frame to a picture
        /// </summary>
        /// <param name="other">The picture</param>
        /// <returns>The hamming distance</returns>
        public int CalculateDistance(PhotoFingerPrintWrapper other)
        {
            return DistanceCalculator.CalculateHammingDistance(PHash, other.PHash);
        }

        /// <summary>
        /// Calculate the distance from this frame to another frame
        /// </summary>
        /// <param name="other">The other Frame</param>
        /// <returns>The hamming distance</returns>
        public int CalculateDistance(FrameFingerPrintWrapper other)
        {
            return DistanceCalculator.CalculateHammingDistance(PHash, other.PHashCode);
        }

        public bool Equals(PhotoFingerPrintWrapper other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return string.Equals(FilePath, other.FilePath, StringComparison.Ordinal) &&
                PHash == other.PHash &&
                Enumerable.SequenceEqual(EdgeGrayScaleThumb, other.EdgeGrayScaleThumb);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PhotoFingerPrintWrapper);
        }

        public override int GetHashCode()
        {
            return PHash.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} | {1}", FilePath, PHash);
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