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

using Core.Media;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FrameIndexLibrary
{
    internal static class FastGaussianBlur
    {
        #region public methods
        public static WritableLockBitImage Transform(
            WritableLockBitImage sourceImage,
            int radius = 3
        )
        {
            using (var copyOfSourceImage = new WritableLockBitImage(sourceImage))
            {
                WritableLockBitImage outputImage = new WritableLockBitImage(sourceImage.Width, sourceImage.Height);
                int index = 0;
                foreach (int boxWidth in EnumerateBoxesForGauss(radius, 3))
                {
                    if (index % 2 == 0)
                    {
                        PerformSplitBoxBlurAcc(copyOfSourceImage, outputImage, (boxWidth - 1) / 2);
                    }
                    else
                    {
                        // Switching of outputImage and copyOfSourceImage is INTENTIONAL!
                        PerformSplitBoxBlurAcc(outputImage, copyOfSourceImage, (boxWidth - 1) / 2);
                    }
                    index++;
                }

                return outputImage;
            }
        }
        #endregion
        #region private methods
        /// <summary>
        /// Calculates the boxes needed for a gaussian blur. Taken from: http://blog.ivank.net/fastest-gaussian-blur.html
        /// Based off of: http://www.peterkovesi.com/papers/FastGaussianSmoothing.pdf 
        /// </summary>
        private static IEnumerable<int> EnumerateBoxesForGauss(int stdDeviation, int numBoxes)
        {
            double widthIdeal = Math.Sqrt((12 * stdDeviation * stdDeviation / numBoxes) + 1);  // Ideal averaging filter width 
            int widthL = (int)Math.Floor(widthIdeal);

            if (widthL % 2 == 0)
            {
                widthL--;
            };

            int widthU = widthL + 2;
            double mIdeal = (12 * stdDeviation * stdDeviation - numBoxes * widthL * widthL - 4 * numBoxes * widthL - 3 * numBoxes)
                / (-4 * widthL - 4);
            int roundedIdealBoxLength = (int)Math.Round(mIdeal);
            for (int index = 0; index < numBoxes; index++)
            {
                yield return index < roundedIdealBoxLength ? widthL : widthU;
            }
        }

        private static WritableLockBitImage PerformSplitBoxBlurAcc(
            WritableLockBitImage sourceImage,
            WritableLockBitImage outputImage,
            int radius
        )
        {
            // Must copy pixels from source to output
            CopyPixels(sourceImage, outputImage);

            // The switching of outputImage and souceImage is INTENTIONAL!
            PerformHorizontalBoxBlurAcc(outputImage, sourceImage, radius);
            PerformTotalBoxBlurAcc(sourceImage, outputImage, radius);

            return outputImage;
        }

        private static void PerformHorizontalBoxBlurAcc(WritableLockBitImage sourceImage, WritableLockBitImage outputImage, int radius)
        {
            double iarr = 1 / ((double)radius + radius + 1);
            for (int row = 0; row < sourceImage.Height; row++)
            {
                Color firstPixel = sourceImage.GetPixel(0, row);
                Color lastPixel = sourceImage.GetPixel(sourceImage.Width - 1, row);
                int cumRedValue = (radius + 1) * firstPixel.R;
                int cumGreenValue = (radius + 1) * firstPixel.G;
                int cumBlueValue = (radius + 1) * firstPixel.B;

                int currentLastColIndex = 0; // li
                int currentRadiusColIndex = radius; // ri

                for (int col = 0; col < radius; col++)
                {
                    Color chosenPixel = sourceImage.GetPixel(col, row);
                    cumRedValue += chosenPixel.R;
                    cumGreenValue += chosenPixel.G;
                    cumBlueValue += chosenPixel.B;
                }

                for (int col = 0; col <= radius; col++)
                {
                    Color chosenPixel = sourceImage.GetPixel(currentRadiusColIndex, row);
                    cumRedValue += chosenPixel.R - firstPixel.R;
                    cumGreenValue += chosenPixel.G - firstPixel.G;
                    cumBlueValue += chosenPixel.B - firstPixel.B;
                    currentRadiusColIndex++;

                    outputImage.SetPixel(
                        col,
                        row,
                        Color.FromArgb(
                            (int)Math.Round(cumRedValue * iarr),
                            (int)Math.Round(cumGreenValue * iarr),
                            (int)Math.Round(cumBlueValue * iarr)
                        )
                    );
                }

                for (int col = radius + 1; col < sourceImage.Width - radius; col++)
                {
                    Color chosenRadiusPixel = sourceImage.GetPixel(currentRadiusColIndex, row);
                    Color chosenLastPixel = sourceImage.GetPixel(currentLastColIndex, row);
                    cumRedValue += chosenRadiusPixel.R - chosenLastPixel.R;
                    cumGreenValue += chosenRadiusPixel.G - chosenLastPixel.G;
                    cumBlueValue += chosenRadiusPixel.B - chosenLastPixel.B;
                    currentRadiusColIndex++;
                    currentLastColIndex++;

                    outputImage.SetPixel(
                        col,
                        row,
                        Color.FromArgb(
                            (int)Math.Round(cumRedValue * iarr),
                            (int)Math.Round(cumGreenValue * iarr),
                            (int)Math.Round(cumBlueValue * iarr)
                        )
                    );
                }

                for (int col = sourceImage.Width - radius; col < sourceImage.Width; col++)
                {
                    Color chosenLastPixel = sourceImage.GetPixel(currentLastColIndex, row);
                    cumRedValue += lastPixel.R - chosenLastPixel.R;
                    cumGreenValue += lastPixel.G - chosenLastPixel.G;
                    cumBlueValue += lastPixel.B - chosenLastPixel.B;
                    currentLastColIndex++;

                    outputImage.SetPixel(
                        col,
                        row,
                        Color.FromArgb(
                            (int)Math.Round(cumRedValue * iarr),
                            (int)Math.Round(cumGreenValue * iarr),
                            (int)Math.Round(cumBlueValue * iarr)
                        )
                    );
                }
            }
        }

        private static void PerformTotalBoxBlurAcc(WritableLockBitImage sourceImage, WritableLockBitImage outputImage, int radius)
        {
            double iarr = 1 / ((double)radius + radius + 1);
            for (int col = 0; col < sourceImage.Width; col++)
            {
                Color topPixel = sourceImage.GetPixel(col, 0);
                Color bottomPixel = sourceImage.GetPixel(col, sourceImage.Height - 1);

                int cumRedValue = (radius + 1) * topPixel.R;
                int cumGreenValue = (radius + 1) * topPixel.G;
                int cumBlueValue = (radius + 1) * topPixel.B;

                for (int row = 0; row < radius; row++)
                {
                    Color chosenPixel = sourceImage.GetPixel(col, row);
                    cumRedValue += chosenPixel.R;
                    cumGreenValue += chosenPixel.G;
                    cumBlueValue += chosenPixel.B;
                }

                for (int row = 0; row <= radius; row++)
                {
                    Color chosenPixel = sourceImage.GetPixel(col, row + radius);
                    cumRedValue += chosenPixel.R - topPixel.R;
                    cumGreenValue += chosenPixel.G - topPixel.G;
                    cumBlueValue += chosenPixel.B - topPixel.B;

                    outputImage.SetPixel(
                        col,
                        row,
                        Color.FromArgb(
                            (int)Math.Round(cumRedValue * iarr),
                            (int)Math.Round(cumGreenValue * iarr),
                            (int)Math.Round(cumBlueValue * iarr)
                        )
                    );
                }

                for (int row = radius + 1; row < sourceImage.Height - radius; row++)
                {
                    Color radiusPixel = sourceImage.GetPixel(col, radius + row);
                    Color laggingPixel = sourceImage.GetPixel(col, row - radius - 1);
                    cumRedValue += radiusPixel.R - laggingPixel.R;
                    cumGreenValue += radiusPixel.G - laggingPixel.G;
                    cumBlueValue += radiusPixel.B - laggingPixel.B;

                    outputImage.SetPixel(
                        col,
                        row,
                        Color.FromArgb(
                            (int)Math.Round(cumRedValue * iarr),
                            (int)Math.Round(cumGreenValue * iarr),
                            (int)Math.Round(cumBlueValue * iarr)
                        )
                    );
                }

                for (int row = sourceImage.Height - radius; row < sourceImage.Height; row++)
                {
                    Color laggingPixel = sourceImage.GetPixel(col, row - radius);
                    cumRedValue += bottomPixel.R - laggingPixel.R;
                    cumGreenValue += bottomPixel.G - laggingPixel.G;
                    cumBlueValue += bottomPixel.B - laggingPixel.B;

                    outputImage.SetPixel(
                        col,
                        row,
                        Color.FromArgb(
                            (int)Math.Round(cumRedValue * iarr),
                            (int)Math.Round(cumGreenValue * iarr),
                            (int)Math.Round(cumBlueValue * iarr)
                        )
                    );
                }
            }
        }

        private static void CopyPixels(WritableLockBitImage sourceImage, WritableLockBitImage destImage)
        {
            for (int row = 0; row < sourceImage.Height; row++)
            {
                for (int col = 0; col < sourceImage.Width; col++)
                {
                    destImage.SetPixel(col, row, sourceImage.GetPixel(col, row));
                }
            }
        }
        #endregion
    }
}