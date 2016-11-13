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

using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Parses the frame header
    /// </summary>
    public sealed class FrameHeaderParser : HeaderParser
    {
        #region private fields
        private readonly Header _fileLevelHeader;
        #endregion

        #region ctor
        /// <summary>
        /// Construct a new FrameHeaderParser with the file-level header to use if all we parse is 
        /// the top-level "FRAME" deliminator
        /// </summary>
        /// <param name="fileLevelHeader"></param>
        public FrameHeaderParser(Header fileLevelHeader)
        {
            _fileLevelHeader = fileLevelHeader;
        }
        #endregion

        #region protected methods
        protected override string HeaderMagicTag
        {
            get { return "FRAME"; }
        }

        protected override Maybe<Header> TryConstructHeader(
            Maybe<int> width,
            Maybe<int> height,
            Maybe<Ratio> framerate,
            Maybe<Ratio> pixelAspectRatio,
            Maybe<Interlacing> interlacing,
            Maybe<ColorSpace> colorspace,
            IEnumerable<string> comments
        )
        {
            int inputWidth = width.OrElse(_fileLevelHeader.Width);
            int inputHeight = height.OrElse(_fileLevelHeader.Height);
            Ratio inputFramerate = framerate.OrElse(_fileLevelHeader.Framerate);
            Maybe<Ratio> inputPixelAspectRatio = pixelAspectRatio.Or(_fileLevelHeader.PixelAspectRatio);
            Maybe<Interlacing> inputInterlacing = interlacing.Or(_fileLevelHeader.Interlacing);
            Maybe<ColorSpace> inputColorSpace = colorspace.Or(_fileLevelHeader.ColorSpace);
            IEnumerable<string> inputComments = _fileLevelHeader.Comments.Concat(comments);

            return (new FrameHeader(
                inputWidth,
                inputHeight,
                inputFramerate,
                inputPixelAspectRatio,
                inputColorSpace,
                inputInterlacing,
                inputComments) as Header).ToMaybe();
        }
        #endregion
    }
}
