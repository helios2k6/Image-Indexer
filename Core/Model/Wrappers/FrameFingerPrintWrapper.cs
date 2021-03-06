﻿/* 
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
    /// The image fingerprint of an image
    /// </summary>
    public sealed class FrameFingerPrintWrapper :
        IEquatable<FrameFingerPrintWrapper>,
        IMetric<FrameFingerPrintWrapper>,
        IMetric<PhotoFingerPrintWrapper>
    {
        #region public properties
        /// <summary>
        /// The frame that this frame occurs at
        /// </summary>
        public int FrameNumber { get; set; }

        /// <summary>
        /// The pHash code of this frame
        /// </summary>
        public ulong PHashCode { get; set; }

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
            return DistanceCalculator.CalculateHammingDistance(PHashCode, other.PHash);
        }

        /// <summary>
        /// Calculate the distance from this frame to another frame
        /// </summary>
        /// <param name="other">The other Frame</param>
        /// <returns>The hamming distance</returns>
        public int CalculateDistance(FrameFingerPrintWrapper other)
        {
            return DistanceCalculator.CalculateHammingDistance(PHashCode, other.PHashCode);
        }

        /// <summary>
        /// Compares this object with the given object
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(FrameFingerPrintWrapper other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return FrameNumber == other.FrameNumber &&
                PHashCode == other.PHashCode &&
                Enumerable.SequenceEqual(EdgeGrayScaleThumb, other.EdgeGrayScaleThumb);
        }

        /// <summary>
        /// Compares this object with the given object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (EqualsPreamble(obj) == false)
            {
                return false;
            }

            return Equals(obj as FrameFingerPrintWrapper);
        }

        /// <summary>
        /// Gets the hashcode
        /// </summary>
        /// <returns>The hashcode</returns>
        public override int GetHashCode()
        {
            return FrameNumber ^ PHashCode.GetHashCode();
        }

        /// <summary>
        /// Get the string representation of this fingerprint
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} | {1}", FrameNumber, PHashCode);
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
