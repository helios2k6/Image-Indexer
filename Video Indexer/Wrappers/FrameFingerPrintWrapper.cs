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
    /// The image fingerprint of an image
    /// </summary>
    public sealed class FrameFingerPrintWrapper : IEquatable<FrameFingerPrintWrapper>
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
        #endregion

        #region public methods
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
                PHashCode == other.PHashCode;
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
