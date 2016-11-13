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

using Functional.Maybe;
using System;
using System.Collections.Generic;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Represents a header for the frame and not the file
    /// </summary>
    internal sealed class FrameHeader : Header, IEquatable<FrameHeader>
    {
        #region ctor
        /// <summary>
        /// Construct a new FrameHeader
        /// </summary>
        /// <param name="width">The width of the frame</param>
        /// <param name="height">The height of the frame</param>
        /// <param name="framerate">The framerate of the video</param>
        /// <param name="pixelAspectRatio">The pixel aspect ratio</param>
        /// <param name="colorSpace">The colorspace</param>
        /// <param name="interlacing">The frame interlacing parameter</param>
        /// <param name="comments">Any comments sent with this frame</param>
        public FrameHeader(
            int width,
            int height,
            Ratio framerate,
            Maybe<Ratio> pixelAspectRatio,
            Maybe<ColorSpace> colorSpace,
            Maybe<Interlacing> interlacing,
            IEnumerable<string> comments
        ) : base(width, height, framerate, pixelAspectRatio, colorSpace, interlacing, comments)
        {
        }
        #endregion

        #region public methods
        public bool Equals(FrameHeader other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FrameHeader);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
