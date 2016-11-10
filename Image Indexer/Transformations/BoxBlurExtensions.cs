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

using System;
using System.Collections.Generic;

namespace ImageIndexer
{
    public static class BoxBlurExtensions
    {
        /// <summary>
        /// Calculates the boxes needed for a gaussian blur. Taken from: http://blog.ivank.net/fastest-gaussian-blur.html
        /// Based off of: http://www.peterkovesi.com/papers/FastGaussianSmoothing.pdf 
        /// </summary>
        public static IEnumerable<int> EnumerateBoxesForGauss(this IBoxBlur _, int stdDeviation, int numBoxes)
        {
            double widthIdeal = Math.Sqrt((12 * stdDeviation * stdDeviation / numBoxes) + 1);  // Ideal averaging filter width 
            int widthL = (int)Math.Floor(widthIdeal);
            
            if (widthL % 2 == 0) {
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

        public static void CopyPixels(this IBoxBlur _, WritableLockBitImage sourceImage, WritableLockBitImage destImage)
        {
            for (int row = 0; row < sourceImage.Height; row++)
            {
                for (int col = 0; col < sourceImage.Width; col++)
                {
                    destImage.SetPixel(col, row, sourceImage.GetPixel(col, row));
                }
            }
        }
    }
}