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
using ImageIndexer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// A utility class for converting colors from different color planes
    /// </summary>
    internal static class ColorConverters
    {
        #region private classes
        private sealed class YCrCbFrame
        {
            public byte[][] Luma { get; set; }

            public byte[][] Cb { get; set; }

            public byte[][] Cr { get; set; }

            public ColorSpace ColorSpace { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }
        }
        #endregion
        #region public methods
        /// <summary>
        /// Try to convert from YCbCr to RGB colors
        /// </summary>
        /// <param name="sourceColorspace">The colorspace of the luma and chroma</param>
        /// <param name="luma">The luma plane</param>
        /// <param name="blueDifferential">The blue differential (Cb) plane</param>
        /// <param name="redDifferential">The red differential (Cr) plane</param>
        /// <param name="width">The width of the frame</param>
        /// <param name="height">The height of the frame</param>
        /// <returns>An RGB plane if this function succeeds. None otherwise</returns>
        /// <remarks>
        /// FFMPEG outputs 420mpeg2 (where the chroma samples are aligned horizontally, but 
        /// shifted a half-pixel down).
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/dd206750(v=vs.85).aspx
        /// </remarks>
        public static Maybe<WritableLockBitImage> TryConvertFrameToRGB(
            ColorSpace sourceColorspace,
            byte[][] luma,
            byte[][] blueDifferential,
            byte[][] redDifferential,
            int width,
            int height
        )
        {
            var inFrames = new YCrCbFrame
            {
                ColorSpace = sourceColorspace,
                Luma = luma,
                Cb = blueDifferential,
                Cr = redDifferential,
                Width = width,
                Height = height,
            };
            if (Equals(sourceColorspace, ColorSpace.FourFourFour))
            {
                return TryConvertYCbCr444ToRGB(inFrames);
            }

            if (Equals(sourceColorspace, ColorSpace.FourTwoTwo))
            {
                return from horizontalUpconvert in TryConvert422To444(inFrames)
                       select TryConvertYCbCr444ToRGB(horizontalUpconvert);
            }

            if (Equals(sourceColorspace, ColorSpace.FourTwoZeroMpeg2))
            {
                return from verticalUpconvert in TryConvert420To422(inFrames)
                       from horizontalUpconvert in TryConvert422To444(verticalUpconvert)
                       select TryConvertYCbCr444ToRGB(horizontalUpconvert);
            }

            return Maybe<WritableLockBitImage>.Nothing;
        }
        #endregion
        #region private methods
        private static Maybe<YCrCbFrame> TryConvert422To444(
            YCrCbFrame subsampledFrames
        )
        {
            var outFrames = new YCrCbFrame
            {
                ColorSpace = ColorSpace.FourTwoTwo,
                Luma = subsampledFrames.Luma,
                Cb = new byte[subsampledFrames.Height][],
                Cr = new byte[subsampledFrames.Height][],
                Width = subsampledFrames.Width,
                Height = subsampledFrames.Height,
            };

            UpsampleHorizontalResolution(subsampledFrames.Cb, outFrames.Cb);
            UpsampleHorizontalResolution(subsampledFrames.Cr, outFrames.Cr);

            return outFrames.ToMaybe();
        }

        /// <summary>
        /// Vertically upconvert from 4:2:0 to 4:2:2
        /// </summary>
        /// <param name="subsampledFrames"></param>
        /// <returns></returns>
        private static Maybe<YCrCbFrame> TryConvert420To422(
            YCrCbFrame subsampledFrames
        )
        {
            var outFrames = new YCrCbFrame
            {
                ColorSpace = ColorSpace.FourTwoTwo,
                Luma = subsampledFrames.Luma,
                Cb = new byte[subsampledFrames.Height][],
                Cr = new byte[subsampledFrames.Height][],
                Width = subsampledFrames.Width,
                Height = subsampledFrames.Height,
            };

            // Initialize all chroma planes
            int widthOfChromaRow = subsampledFrames.Cb[0].Length;
            for (int row = 0; row < outFrames.Height; row++)
            {
                outFrames.Cb[row] = new byte[widthOfChromaRow];
                outFrames.Cr[row] = new byte[widthOfChromaRow];
            }

            UpsampleVerticalResolution(subsampledFrames.Cb, outFrames.Cb);
            UpsampleVerticalResolution(subsampledFrames.Cr, outFrames.Cr);

            return outFrames.ToMaybe();
        }

        private static Maybe<WritableLockBitImage> TryConvertYCbCr444ToRGB(
            YCrCbFrame inFrame
        )
        {
            return TryConvertYCbCr444ToRGBParallel(inFrame);
        }

        private static Maybe<WritableLockBitImage> TryConvertYCbCr444ToRGBParallel(
            YCrCbFrame inFrame
        )
        {
            WritableLockBitImage frame = new WritableLockBitImage(inFrame.Width, inFrame.Height);
            try
            {
                Parallel.For(0, inFrame.Height, row =>
                {
                    for (int col = 0; col < inFrame.Width; col++)
                    {
                        byte currentLuma = inFrame.Luma[row][col];
                        byte currentCb = inFrame.Cb[row][col];
                        byte currentCr = inFrame.Cr[row][col];
                        frame.SetPixel(col, row, ConvertYUVToRGB(currentLuma, currentCb, currentCr));
                    }
                });
                frame.Lock();
                return frame.ToMaybe();
            }
            catch (Exception)
            {
            }

            frame.Dispose();
            return Maybe<WritableLockBitImage>.Nothing;
        }

        private static Color ConvertYUVToRGB(byte luma, byte blueDifferential, byte redDifferential)
        {
            int c = luma - 16;
            int d = blueDifferential - 128;
            int e = redDifferential - 128;

            int red = Clamp((298 * c + 409 * e + 128) >> 8);
            int green = Clamp((298 * c - 100 * d - 208 * e + 128) >> 8);
            int blue = Clamp((298 * c + 516 * d + 128) >> 8);

            return Color.FromArgb(red, green, blue);
        }

        private static void UpsampleVerticalResolution(
            byte[][] inChromaPlane,
            byte[][] outChromaPlane
        )
        {
            for (int col = 0; col < inChromaPlane[0].Length; col++)
            {
                IEnumerable<byte> upsampledColumn = GetUpsampledLine(GetColumn(inChromaPlane, col), inChromaPlane.Length);
                SetColumn(outChromaPlane, col, upsampledColumn);
            }
        }

        private static void UpsampleHorizontalResolution(
            byte[][] inChromaPlane,
            byte[][] outChromaPlane
        )
        {
            for (int row = 0; row < inChromaPlane.Length; row++)
            {
                outChromaPlane[row] = GetUpsampledLine(inChromaPlane[row], inChromaPlane[row].Length).ToArray();
            }
        }

        private static void SetColumn(byte[][] expandedChromaPlane, int columnIndex, IEnumerable<byte> columnToSet)
        {
            int currentYPosition = 0;
            foreach (byte columnValue in columnToSet)
            {
                expandedChromaPlane[currentYPosition][columnIndex] = columnValue;
                currentYPosition++;
            }
        }

        private static IEnumerable<byte> GetColumn(byte[][] chromaPlane, int columnIndex)
        {
            for (int y = 0; y < chromaPlane.Length; y++)
            {
                yield return chromaPlane[y][columnIndex];
            }
        }

        private static IEnumerable<byte> GetUpsampledLine(IEnumerable<byte> chromaLine, int lengthOfChromaLine)
        {
            int currentIndex = 0;
            foreach (byte sample in chromaLine)
            {
                yield return sample;
                yield return GetUpsampledByte(chromaLine, currentIndex, lengthOfChromaLine);

                currentIndex++;
            }
        }

        private static byte GetUpsampledByte(
            IEnumerable<byte> chromaLine,
            int chromaPlaneIndex,
            int lengthOfChromaLine
        )
        {
            // Check to see if we're at the top edge
            if (chromaPlaneIndex == 0)
            {
                return Clamp((byte)((9 * (chromaLine.ElementAt(0) + chromaLine.ElementAt(1)) - (chromaLine.ElementAt(0) + chromaLine.ElementAt(2)) + 8) >> 4));
            }
            // Check to see if we're right before the bottom edge; gotta special case this second-to-last line
            else if (chromaPlaneIndex == lengthOfChromaLine - 2)
            {
                return Clamp((byte)((9 * (chromaLine.ElementAt(chromaPlaneIndex) + chromaLine.ElementAt(chromaPlaneIndex + 1)) - (chromaLine.ElementAt(chromaPlaneIndex - 1) + chromaLine.ElementAt(chromaPlaneIndex + 1)) + 8) >> 4));
            }
            // Check to see if we're at the bottom edge
            else if (chromaPlaneIndex == lengthOfChromaLine - 1)
            {
                return Clamp((byte)((9 * (chromaLine.ElementAt(chromaPlaneIndex) + chromaLine.ElementAt(chromaPlaneIndex)) - (chromaLine.ElementAt(chromaPlaneIndex - 1) + chromaLine.ElementAt(chromaPlaneIndex)) + 8) >> 4));
            }
            // We're somewhere in the middle, which is generalizable
            return Clamp((byte)((9 * (chromaLine.ElementAt(chromaPlaneIndex) + chromaLine.ElementAt(chromaPlaneIndex + 1)) - (chromaLine.ElementAt(chromaPlaneIndex - 1) + chromaLine.ElementAt(chromaPlaneIndex + 2)) + 8) >> 4));
        }

        /// <summary>
        /// Specific clamp function for limiting these values to the range of 1 byte
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static byte Clamp(byte val)
        {
            if (val.CompareTo(0) < 0) return 0;
            else if (val.CompareTo(255) > 0) return 255;
            else return val;
        }

        /// <summary>
        /// Specific clamp function for limiting these values to the range of 1 byte
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static int Clamp(int val)
        {
            if (val.CompareTo(0) < 0) return 0;
            else if (val.CompareTo(255) > 0) return 255;
            else return val;
        }
        #endregion
    }
}
