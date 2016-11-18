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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Parses a video file from a bitstream
    /// </summary>
    internal sealed class VideoFileParser : IDisposable
    {
        #region private fields
        private readonly string _videoFilePath;
        private readonly Stream _fileStream;

        private bool _hasRun;
        private bool _disposedValue;
        #endregion

        #region ctor
        /// <summary>
        /// Construct a video file parser from the given file path
        /// </summary>
        /// <param name="videoFilePath">The path to the video file</param>
        public VideoFileParser(string videoFilePath)
        {
            _videoFilePath = videoFilePath;
            _fileStream = new FileStream(_videoFilePath, FileMode.Open, FileAccess.Read);
            _hasRun = false;
            _disposedValue = false;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Attempts to parse a video file
        /// </summary>
        /// <returns>A newly parsed video file or None</returns>
        public Maybe<VideoFile> TryParseVideoFile()
        {
            if (_hasRun)
            {
                throw new InvalidOperationException("This video parser has been executed once already");
            }
            _hasRun = true;

            // First read file header
            Maybe<Header> fileHeader = FileHeaderParser.Instance.TryParseHeader(_fileStream);
            if (fileHeader.IsNothing())
            {
                return Maybe<VideoFile>.Nothing;
            }

            // Second read the frames
            return new VideoFile(
                fileHeader.Value,
                EnumerateVideoFrames(fileHeader.Value, _fileStream),
                CalculateByteOffsets(fileHeader.Value, _fileStream.Position, _fileStream.Length),
                _videoFilePath
            ).ToMaybe();
        }

        public void Dispose()
        {
            if (!_disposedValue)
            {
                _fileStream.Dispose();
                _disposedValue = true;
            }
        }
        #endregion

        #region private methods
        private IEnumerable<long> CalculateByteOffsets(Header fileHeader, long indexOfFirstFrame, long sizeOfFile)
        {
            Maybe<ColorSpace> colorSpaceMaybe = fileHeader.ColorSpace;
            if (colorSpaceMaybe.IsNothing())
            {
                yield break;
            }

            ColorSpace colorSpace = colorSpaceMaybe.Value;
            if (colorSpace.Equals(ColorSpace.FourFourFour))
            {
                long frameSize = (fileHeader.Height * fileHeader.Width * 3) +
                    FrameHeaderParser.HeaderMagicTagLength + 1; // The last byte is the 0x0A frame ending byte
                for (long index = indexOfFirstFrame; index < sizeOfFile; index += frameSize)
                {
                    yield return index;
                }
            }
            else if (colorSpace.Equals(ColorSpace.FourTwoZero) || colorSpace.Equals(ColorSpace.FourTwoZeroMpeg2))
            {
                long frameSize = (fileHeader.Height * fileHeader.Width + (1 / 2 * (fileHeader.Height * fileHeader.Width))) +
                    FrameHeaderParser.HeaderMagicTagLength + 1; // The last byte is the 0x0A frame ending byte
                for (long index = indexOfFirstFrame; index < sizeOfFile; index += frameSize)
                {
                    yield return index;
                }
            }
        }

        private IEnumerable<VideoFrame> EnumerateVideoFrames(Header fileHeader, Stream stream)
        {
            Maybe<VideoFrame> parsedVideoFrame = Maybe<VideoFrame>.Nothing;
            VideoFrameParser parser = new VideoFrameParser(fileHeader);
            do
            {
                parsedVideoFrame = parser.TryParseVideoFrame(stream);
                if (parsedVideoFrame.IsSomething())
                {
                    yield return parsedVideoFrame.Value;
                }
            } while (parsedVideoFrame.IsSomething());
        }
        #endregion
    }
}
