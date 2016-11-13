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
using System.IO;
using VideoIndexer.Utils;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Parses the frame from a bitstream
    /// </summary>
    internal sealed class VideoFrameParser
    {
        #region private fields
        private readonly Header _header;
        #endregion

        #region private properties
        private ColorSpace DetectedColorSpace
        {
            get
            {
                return _header.ColorSpace.HasValue
                    ? _header.ColorSpace.Value
                    : ColorSpace.FourTwoZero;
            }
        }
        #endregion

        #region ctor
        public VideoFrameParser(Header header)
        {
            _header = header;
        }
        #endregion

        #region public methods
        public Maybe<VideoFrame> TryParseVideoFrame(Stream rawStream)
        {
            using (var rewindGuard = new RewindGuard(rawStream))
            {
                var frameHeaderParser = new FrameHeaderParser(_header);
                Maybe<Header> frameHeaderMaybe = frameHeaderParser.TryParseHeader(rawStream); // We don't do anything with the frame, so who cares?
                if (frameHeaderMaybe.IsNothing())
                {
                    // We have to at least succeed
                    return Maybe<VideoFrame>.Nothing;
                }

                Maybe<VideoFrame> videoFrame = from lumaPlane in TryReadLumaPlane(rawStream)
                                               from blueDiff in TryReadChromaPlane(rawStream)
                                               from redDiff in TryReadChromaPlane(rawStream)
                                               from frameHeader in frameHeaderMaybe
                                               from colorSpace in frameHeader.ColorSpace
                                               from colorMatrix in ColorConverters.TryConvertFrameToRGB(
                                                   colorSpace,
                                                   lumaPlane,
                                                   blueDiff,
                                                   redDiff,
                                                   _header.Width,
                                                   _header.Height
                                               )
                                               select new VideoFrame(frameHeader, colorMatrix);

                videoFrame.Apply(_ =>
                {
                    rewindGuard.DoNotRewind();
                });

                return videoFrame;
            }
        }
        #endregion

        #region private methods
        private Maybe<byte[][]> TryReadLumaPlane(Stream rawStream)
        {
            var lumaPlaneBuffer = new byte[_header.Width][];
            for (int row = 0; row < _header.Height; row++)
            {
                lumaPlaneBuffer[row] = new byte[_header.Width];
                int readBytes = rawStream.Read(lumaPlaneBuffer[row], 0, _header.Width);
                if (readBytes != _header.Width)
                {
                    return Maybe<byte[][]>.Nothing;
                }

            }
            return lumaPlaneBuffer.ToMaybe();
        }

        private Maybe<byte[][]> TryReadChromaPlane(Stream rawStream)
        {
            // TODO: Account for 10bit pixels
            if (Equals(DetectedColorSpace, ColorSpace.FourFourFour))
            {
                // 4:4:4
                return ReadPlane(rawStream, _header.Width, _header.Height);
            }
            else if (Equals(DetectedColorSpace, ColorSpace.FourTwoTwo))
            {
                // 4:2:2
                return ReadPlane(rawStream, _header.Width / 2, _header.Height);
            }
            else if (Equals(DetectedColorSpace, ColorSpace.FourTwoZeroMpeg2))
            {
                // 4:2:0
                return ReadPlane(rawStream, _header.Width / 2, _header.Height / 2);
            }

            return Maybe<byte[][]>.Nothing;
        }

        private static Maybe<byte[][]> ReadPlane(Stream rawStream, int width, int height)
        {
            var planeBuffer = new byte[height][];
            for (int row = 0; row < height; row++)
            {
                planeBuffer[row] = new byte[width];
                int readBytes = rawStream.Read(planeBuffer[row], 0, width);
                if (readBytes != width)
                {
                    return Maybe<byte[][]>.Nothing;
                }
            }

            return planeBuffer.ToMaybe();
        }
        #endregion
    }
}
