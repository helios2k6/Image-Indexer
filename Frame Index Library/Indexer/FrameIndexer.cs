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

using Core.Compression;
using Core.Media;
using System.Collections.Generic;
using System.Drawing;
using System;

namespace FrameIndexLibrary
{
    /// <summary>
    /// Indexes a video
    /// </summary>
    public static class FrameIndexer
    {
        #region private fields
        private static readonly int FingerPrintWidth = 32;
        #endregion

        #region public methods
        /// <summary>
        /// Indexes a single frame
        /// </summary>
        /// <param name="frame">The frame to index</param>
        /// <returns>An indexed frame</returns>
        public static Tuple<ulong, byte[]> IndexFrame(Image frame)
        {
            return CalcluateFramePerceptionHash(frame, true);
        }

        /// <summary>
        /// Calculates just the frame preception hashcode and skips all of the
        /// expensive sobel filter code
        /// </summary>
        public static ulong CalculateFramePerceptionHashOnly(Image frame)
        {
            return CalcluateFramePerceptionHash(frame, false).Item1;
        }
        #endregion

        #region private methods
        private static Tuple<ulong, byte[]> CalcluateFramePerceptionHash(Image frame, bool shouldDoEdgeDetection)
        {
            using (WritableLockBitImage resizedImage = new WritableLockBitImage(ResizeTransformation.Transform(frame, FingerPrintWidth, FingerPrintWidth)))
            using (WritableLockBitImage grayscaleImage = GreyScaleTransformation.TransformInPlace(resizedImage))
            using (WritableLockBitImage blurredImage = FastGaussianBlur.Transform(grayscaleImage))
            {
                double[,] dctMatrix = FastDCTCalculator.Transform(blurredImage);
                double medianOfDCTValue = CalculateMedianDCTValue(dctMatrix);
                ulong hashCode = ConstructHashCode(dctMatrix, medianOfDCTValue);
                if (shouldDoEdgeDetection)
                {
                    using (WritableLockBitImage sobelImage = SobelFilter.TransformWithGrayScaleImage(grayscaleImage))
                    using (WritableLockBitImage quantisizedImage = QuantisizingFilter.TransformInPlace(sobelImage, 1))
                    using (WritableLockBitImage resizedQuantisizedImage = ResizeTransformation.Transform(quantisizedImage, 16, 16))
                    {
                        return Tuple.Create(hashCode, CalculateGrayScaleThumbnail(resizedQuantisizedImage));
                    }
                }

                return Tuple.Create<ulong, byte[]>(hashCode, null);
            }
        }

        private static byte[] CalculateGrayScaleThumbnail(WritableLockBitImage image)
        {
            byte[] buffer = new byte[image.Width * image.Height];
            for (int row = 0; row < image.Height; row++)
            {
                for (int col = 0; col < image.Width; col++)
                {
                    buffer[row * image.Width + col] = image.GetPixel(col, row).R;
                }
            }

            return ByteCompressor.Compress(buffer);
        }

        private static double CalculateMedianDCTValue(double[,] dctMatrix)
        {
            var listOfDoubles = new List<double>(64);
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    listOfDoubles.Add(dctMatrix[row, col]);
                }
            }

            listOfDoubles.Sort();
            return listOfDoubles[32];
        }

        private static ulong ConstructHashCode(double[,] dctMatrix, double averageGreyScaleValue)
        {
            ulong currentHashValue = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (dctMatrix[y, x] >= averageGreyScaleValue)
                    {
                        ulong shiftedBit = ((ulong)1) << (x * y);
                        currentHashValue = currentHashValue | shiftedBit;
                    }
                }
            }

            return currentHashValue;
        }
        #endregion
    }
}
