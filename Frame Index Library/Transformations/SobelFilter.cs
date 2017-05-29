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
    internal static class SobelFilter
    {
        #region private fields
        #endregion

        #region public methods
        public static WritableLockBitImage Transform(WritableLockBitImage sourceImage)
        {
            return null;
        }
        #endregion

        #region private methods
        private static int Convolute(int[,] sourceMatrix, int[,] filter)
        {
            if (sourceMatrix.GetLength(0) != filter.GetLength(0) || sourceMatrix.GetLength(1) != filter.GetLength(1))
            {
                throw new ArgumentException("Cannot convolute two matrices if they are not the same width and height");
            }

            int numRows = sourceMatrix.GetLength(0);
            int numCols = sourceMatrix.GetLength(1);
            int sum = 0;
            // Reference: https://en.wikipedia.org/wiki/Kernel_(image_processing)#Convolution
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    sum += sourceMatrix[row, col] * filter[numRows - row - 1, numCols - col - 1];
                }
            }

            return sum;
        }
        #endregion
    }
}