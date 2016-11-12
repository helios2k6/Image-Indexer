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
using System.Drawing;
using System.Linq;

namespace ImageIndexer
{
    /// <summary>
    /// Represents a macroblock of pixels in a fingerprint
    /// </summary>
    public sealed class MacroblockWrapper : IEquatable<MacroblockWrapper>
    {
        #region public properties
        /// <summary>
        /// The pixels of this macroblock
        /// </summary>
        public int[] GreyScalePixels { get; set; }
        /// <summary>
        /// The width of this macroblock
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of this macroblock
        /// </summary>
        public int Height { get; set; }
        #endregion

        #region public methods
        /// <summary>
        /// Compare the equality of two macroblocks
        /// </summary>
        /// <param name="other">The other macroblock</param>
        /// <returns>True if the two objects are equal. False otherwise</returns>
        public bool Equals(MacroblockWrapper other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            if (Width != other.Width || Height != other.Height)
            {
                return false;
            }

            if (GreyScalePixels.Length != other.GreyScalePixels.Length)
            {
                return false;
            }

            for (int i = 0; i < GreyScalePixels.Length; i++)
            {
                if (GreyScalePixels[i] != other.GreyScalePixels[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compare the equality of two objects
        /// </summary>
        /// <param name="obj">The other object to compare against this one</param>
        /// <returns>True if the objects are equal. False otherwise</returns>
        public override bool Equals(object obj)
        {
            if (EqualsPreamble(obj) == false)
            {
                return false;
            }

            return Equals(obj as MacroblockWrapper);
        }

        /// <summary>
        /// Get the hashcode of this object
        /// </summary>
        /// <returns>The hashcode</returns>
        public override int GetHashCode()
        {
            int pixelsHashCode = GreyScalePixels != null
                ? GreyScalePixels.Aggregate(0, (acc, pixel) => acc + pixel)
                : 0;
            return Width ^
                Height ^
                pixelsHashCode;
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
