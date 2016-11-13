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
using System.IO;
using System.Linq;
using VideoIndexer.Utils;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Base class for all header parsers; specifically the header and frame parsers
    /// </summary>
    public abstract class HeaderParser
    {
        #region private fields
        private const byte ParameterSeparator = 0x20; // The ASCII code for space (" ") 
        private const byte HeaderEndByte = 0x0A;
        private const char CommentParameter = 'X';
        private const char WidthParameter = 'W';
        private const char HeightParemter = 'H';
        private const char FrameRateParameter = 'F';
        private const char InterlaceParameter = 'I';
        private const char PixelAspectRatioParameter = 'A';
        private const char ColorSpaceParameter = 'C';
        #endregion

        #region public properties
        /// <summary>
        /// The header magic tag for this type of header
        /// </summary>
        protected abstract string HeaderMagicTag { get; }
        #endregion

        #region public methods
        public Maybe<Header> TryParseHeader(Stream rawStream)
        {
            using (var rewindGuard = new RewindGuard(rawStream))
            {
                if (TryReadHeaderMagicTag(rawStream, HeaderMagicTag) == false)
                {
                    return Maybe<Header>.Nothing;
                }

                Maybe<Header> candidateHeader = TryReadHeaderParameters(rawStream);
                candidateHeader.Apply(header =>
                {
                    rewindGuard.DoNotRewind();
                });

                return candidateHeader;
            }
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Attempt to construct a header given the arguments 
        /// </summary>
        /// <param name="width">The width of the video</param>
        /// <param name="height">The height of the video</param>
        /// <param name="framerate">The framerate of the video</param>
        /// <param name="pixelAspectRatio">The pixel aspect ratio</param>
        /// <param name="interlacing">The frame interlacing method</param>
        /// <param name="colorspace">The colorspace</param>
        /// <param name="comments">The comments in the header</param>
        /// <returns>A fully constructed header or None if it failed</returns>
        protected abstract Maybe<Header> TryConstructHeader(
            Maybe<int> width,
            Maybe<int> height,
            Maybe<Ratio> framerate,
            Maybe<Ratio> pixelAspectRatio,
            Maybe<Interlacing> interlacing,
            Maybe<ColorSpace> colorspace,
            IEnumerable<string> comments
        );
        #endregion

        #region private methods
        private bool TryReadHeaderMagicTag(Stream rawStream, string headerMagicTag)
        {
            var buffer = new byte[headerMagicTag.Length];
            int readBytes = rawStream.Read(buffer, 0, headerMagicTag.Length);
            if (readBytes == headerMagicTag.Length)
            {
                var readHeader = new string(buffer.Select(Convert.ToChar).ToArray());
                if (string.Equals(headerMagicTag, readHeader, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to parse the parameters of the stream to form a header. If the stream is malformed, this
        /// returns None
        /// </summary>
        /// <param name="rawStream">The raw stream of bytes to parse</param>
        /// <returns>An optional Header</returns>
        private Maybe<Header> TryReadHeaderParameters(Stream rawStream)
        {
            Maybe<IEnumerable<string>> parameters = TryGetParameters(rawStream);
            if (parameters.IsNothing())
            {
                return Maybe<Header>.Nothing;
            }

            IEnumerable<string> comments = Enumerable.Empty<string>();
            Maybe<int> width = Maybe<int>.Nothing;
            Maybe<int> height = Maybe<int>.Nothing;
            Maybe<Ratio> framerate = Maybe<Ratio>.Nothing;
            Maybe<Ratio> pixelAspectRatio = Maybe<Ratio>.Nothing;
            Maybe<Interlacing> interlacing = Maybe<Interlacing>.Nothing;
            Maybe<ColorSpace> colorSpace = Maybe<ColorSpace>.Nothing;
            foreach (string currentFullParameter in parameters.Value)
            {
                char parameter = currentFullParameter.First();
                string parameterBody = currentFullParameter.Substring(1);
                switch (parameter)
                {
                    case CommentParameter:
                        {
                            comments = comments.Append(parameterBody);
                        }
                        break;
                    case WidthParameter:
                        {
                            int possibleWidth = -1;
                            if (int.TryParse(parameterBody, out possibleWidth))
                            {
                                width = possibleWidth.ToMaybe();
                            }
                        }
                        break;
                    case HeightParemter:
                        {
                            int possibleHeight = -1;
                            if (int.TryParse(parameterBody, out possibleHeight))
                            {
                                height = possibleHeight.ToMaybe();
                            }
                        }
                        break;
                    case FrameRateParameter:
                        {
                            framerate = Ratio.TryParse(parameterBody);
                        }
                        break;
                    case InterlaceParameter:
                        {
                            interlacing = Interlacing.TryParseInterlacing(parameterBody);
                        }
                        break;
                    case PixelAspectRatioParameter:
                        {
                            pixelAspectRatio = Ratio.TryParse(parameterBody);
                        }
                        break;
                    case ColorSpaceParameter:
                        {
                            colorSpace = ColorSpace.TryParse(parameterBody);
                        }
                        break;
                    default:
                        {
                            return Maybe<Header>.Nothing;
                        }
                }
            }

            return TryConstructHeader(width, height, framerate, pixelAspectRatio, interlacing, colorSpace, comments);
        }

        private Maybe<IEnumerable<string>> TryGetParameters(Stream rawStream)
        {
            int currentByte = rawStream.ReadByte();
            bool currentlyReadingParameter = false;
            bool isFirstParameterSeparator = true;
            var byteBuffer = new List<int>(32);
            var parameterList = new List<string>();
            while (currentByte != HeaderEndByte || currentByte == -1) // If we're at the end of the header or the stream, don't iterate
            {
                if (currentlyReadingParameter && currentByte != ParameterSeparator)
                {
                    byteBuffer.Add(currentByte);
                }
                else
                {
                    if (currentByte == ParameterSeparator)
                    {
                        // Prevent us from emitting an empty string as a parameter
                        if (isFirstParameterSeparator)
                        {
                            isFirstParameterSeparator = false;
                            currentlyReadingParameter = true;
                            currentByte = rawStream.ReadByte();
                            continue;
                        }

                        // Yield the current set of bytes as a string
                        parameterList.Add(new string(byteBuffer.Select(Convert.ToChar).ToArray()));

                        // Clear the buffer
                        byteBuffer.Clear();
                    }
                    else
                    {
                        return Maybe<IEnumerable<string>>.Nothing;
                    }
                }

                currentByte = rawStream.ReadByte();
            }

            return (parameterList as IEnumerable<string>).ToMaybe();
        }
        #endregion
    }
}
