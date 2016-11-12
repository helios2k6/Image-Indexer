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
using System.Drawing;

namespace ImageIndexer
{
    public static class Indexer
    {
        #region private fields
        private static readonly int MacroblockLength = 4;
        private static readonly int FingerPrintWidth = 32;
        private static readonly int BlurRadius = 15;
        #endregion

        #region public methods
        public static VideoFingerPrintWrapper IndexVideo(IEnumerable<Image> frames)
        {
            return null;
        }
        #endregion

        #region private methods
        private static FrameFingerPrintWrapper IndexFrame(Image frame)
        {
            using (Image resizedImage = ResizeTransformation.Transform(frame, FingerPrintWidth, FingerPrintWidth))
            using (Image blackAndWhiteImage = GreyScaleTransformation.Transform(resizedImage))
            using (Image dctImage = DCTGreyScaleTransformation.Transform(blackAndWhiteImage))
            {
                double averageGreyScaleValue = CalculateAverageOfDCT(dctImage);
                ulong hashcode = ConstructHashCode(dctImage, averageGreyScaleValue);
            }

            return null;
        }

        private static double CalculateAverageOfDCT(Image dctImage)
        {
            using (var lockbitImage = new WritableLockBitImage(dctImage))
            {
                int runningSum = 0;
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        // Ignore the DC coefficient
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        runningSum += lockbitImage.GetPixel(x, y).R;
                    }
                }

                return runningSum / 63.0;
            }
        }

        private static ulong ConstructHashCode(Image dctImage, double averageGreyScaleValue)
        {
            using (var lockbitImage = new WritableLockBitImage(dctImage))
            {
                ulong currentHashValue = 0;
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (lockbitImage.GetPixel(x, y).R < averageGreyScaleValue)
                        {
                            ulong shiftedBit = ((ulong)1) << (x * y);
                            currentHashValue = currentHashValue | shiftedBit;
                        }
                    }
                }

                return currentHashValue;
            }

        }
        #endregion
    }
}
