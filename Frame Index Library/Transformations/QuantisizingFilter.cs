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
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace FrameIndexLibrary
{
    /// <summary>
    /// Quantisizes the image for a given value by rounding up or down
    /// based on the median
    /// </summary>
    internal static class QuantisizingFilter
    {
        /// <summary>
        /// Quantisize a photo by ceiling or flooring each value depending one whether it's above or below the median value
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="outputImage"></param>
        /// <returns></returns>
        public static WritableLockBitImage Transform(WritableLockBitImage sourceImage, WritableLockBitImage outputImage)
        {
            Color medianColor = GetMedianColorValue(sourceImage);
            for (int row = 0; row < sourceImage.Height; row++)
            {
                for (int col = 0; col < sourceImage.Width; col++)
                {
                    // Quantisize the individual channels
                    Color currentColor = sourceImage.GetPixel(col, row);
                    byte outputRed, outputGreen, outputBlue;
                    if (currentColor.R < medianColor.R)
                    {
                        outputRed = 0;
                    }
                    else
                    {
                        outputRed = byte.MaxValue;
                    }

                    if (currentColor.G < medianColor.G)
                    {
                        outputGreen = 0;
                    }
                    else
                    {
                        outputGreen = byte.MaxValue;
                    }

                    if (currentColor.B < medianColor.B)
                    {
                        outputBlue = 0;
                    }
                    else
                    {
                        outputBlue = byte.MaxValue;
                    }

                    outputImage.SetPixel(col, row, Color.FromArgb(outputRed, outputGreen, outputBlue));
                }
            }

            return outputImage;
        }

        public static WritableLockBitImage TransformInPlace(WritableLockBitImage sourceImage)
        {
            return Transform(sourceImage, sourceImage);
        }

        private static Color GetMedianColorValue(WritableLockBitImage sourceImage)
        {
            var setOfRedColorValues = new HashSet<int>();
            var setOfGreenColorValues = new HashSet<int>();
            var setOfBlueColorValues = new HashSet<int>();

            for (int row = 0; row < sourceImage.Height; row++)
            {
                for (int col = 0; col < sourceImage.Width; col++)
                {
                    Color color = sourceImage.GetPixel(col, row);
                    setOfRedColorValues.Add(color.R);
                    setOfGreenColorValues.Add(color.G);
                    setOfBlueColorValues.Add(color.B);
                }
            }

            List<int> listOfRedColorValues = setOfRedColorValues.ToList();
            List<int> listOfGreenColorValues = setOfRedColorValues.ToList();
            List<int> listOfBlueColorValues = setOfRedColorValues.ToList();

            listOfRedColorValues.Sort();
            listOfGreenColorValues.Sort();
            listOfBlueColorValues.Sort();

            int count = listOfRedColorValues.Count;
            int medianRed, medianGreen, medianBlue;
            if (count % 2 == 0)
            {
                medianRed = (int)Math.Round((listOfRedColorValues[count / 2] + listOfRedColorValues[count / 2 - 1]) / 2.0);
                medianGreen = (int)Math.Round((listOfGreenColorValues[count / 2] + listOfGreenColorValues[count / 2 - 1]) / 2.0);
                medianBlue = (int)Math.Round((listOfBlueColorValues[count / 2] + listOfBlueColorValues[count / 2 - 1]) / 2.0);
            }
            else
            {
                medianRed = listOfRedColorValues[count / 2];
                medianGreen = listOfGreenColorValues[count / 2];
                medianBlue = listOfBlueColorValues[count / 2];
            }

            return Color.FromArgb(medianRed, medianGreen, medianBlue);
        }
    }
}