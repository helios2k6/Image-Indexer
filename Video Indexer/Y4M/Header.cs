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

using Functional.Maybe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Represents the Y4M file header and includes all of the information that is 
    /// associated with the Y4M file
    /// </summary>
    internal abstract class Header : IEquatable<Header>
    {
        #region public properties
        /// <summary>
        /// The width of the video
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the video
        /// </summary> 
        public int Height { get; private set; }

        /// <summary>
        /// The framerate of the video
        /// </summary>
        public Ratio Framerate { get; private set; }

        /// <summary>
        /// The pixel aspect ratio of the video, if it's available
        /// </summary>
        public Maybe<Ratio> PixelAspectRatio { get; private set; }

        /// <summary>
        /// Represents the colorspace this video uses
        /// </summary>
        public Maybe<ColorSpace> ColorSpace { get; private set; }

        /// <summary>
        /// The interlacing setting used
        /// </summary>
        public Maybe<Interlacing> Interlacing { get; private set; }

        /// <summary>
        /// The comments that accompanied this header
        /// </summary>
        public IEnumerable<string> Comments { get; private set; }
        #endregion

        #region ctor
        protected Header(
            int width,
            int height,
            Ratio framerate,
            Maybe<Ratio> pixelAspectRatio,
            Maybe<ColorSpace> colorSpace,
            Maybe<Interlacing> interlacing,
            IEnumerable<string> comments
        )
        {
            Width = width;
            Height = height;
            Framerate = framerate;
            PixelAspectRatio = pixelAspectRatio;
            ColorSpace = colorSpace;
            Interlacing = interlacing;
            Comments = comments;
        }
        #endregion

        #region public methods
        public bool Equals(Header other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return Equals(Width, other.Width) &&
                Equals(Height, other.Height) &&
                Equals(Framerate, other.Framerate) &&
                Equals(PixelAspectRatio, other.PixelAspectRatio) &&
                Equals(ColorSpace, other.ColorSpace) &&
                Enumerable.SequenceEqual(Comments, other.Comments);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Header);
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode() ^
                Height.GetHashCode() ^
                Framerate.GetHashCode() ^
                PixelAspectRatio.GetHashCode() ^
                ColorSpace.GetHashCode() ^
                Comments.Aggregate(0, (agg, s) => agg ^ s.GetHashCode(), i => i);
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
