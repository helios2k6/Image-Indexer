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

namespace FrameIndexLibrary
{
    /// <summary>
    /// Performs the Sobel Filter on a given image
    /// </summary>
    /// <remarks>
    /// Implementation from https://en.wikipedia.org/wiki/Sobel_operator with tips from 
    /// https://github.com/petermlm/SobelFilter/blob/master/src/sobel.c
    /// </remarks>
    internal static class SobelFilter
    {
        #region private fields
        private static readonly int[] HorizontalMatrixStep1 = { 1, 0, -1 };

        private static readonly int[] HorizontalMatrixStep2 = { 1, 2, 1 };

        private static readonly int[] VerticalMatrixStep1 = { 1, 2, 1 };

        private static readonly int[] VerticalMatrixStep2 = { 1, 0, -1 };
        #endregion

        #region public methods
        /// <summary>
        /// Performs the Sobel Filter on the given gray scale image
        /// </summary>
        /// <param name="grayScaleImage"></param>
        /// <returns></returns>
        public static WritableLockBitImage TransformWithGrayScaleImage(WritableLockBitImage grayScaleImage)
        {
            return ApplySobelFilter(grayScaleImage);
        }
        #endregion

        #region private methods
        private static WritableLockBitImage ApplySobelFilter(WritableLockBitImage sourceImage)
        {
            return CalculateContour(
                CalculateHorizontalSobelMatrix(sourceImage),
                CalculateVerticalSobelMatrix(sourceImage),
                sourceImage.Width,
                sourceImage.Height
             );
        }

        private static WritableLockBitImage CalculateContour(int[] horizontalMatrix, int[] verticalMatrix, int width, int height)
        {
            WritableLockBitImage outputImage = new WritableLockBitImage(width, height);
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int horizontalValue = horizontalMatrix[row * width + col];
                    int verticalValue = verticalMatrix[row * width + col];
                    byte pixelValue = (byte)Math.Round(Math.Sqrt(
                        (horizontalValue * horizontalValue) +
                        (verticalValue * verticalValue)
                    ));

                    outputImage.SetPixel(col, row, Color.FromArgb(pixelValue, pixelValue, pixelValue));
                }
            }
            return outputImage;
        }

        private static int[] CalculateHorizontalSobelMatrix(WritableLockBitImage sourceImage)
        {
            return CoreCalculateSobelMatrix(sourceImage, HorizontalMatrixStep1, HorizontalMatrixStep2);
        }

        private static int[] CalculateVerticalSobelMatrix(WritableLockBitImage sourceImage)
        {
            return CoreCalculateSobelMatrix(sourceImage, VerticalMatrixStep1, VerticalMatrixStep2);
        }

        private static int[] CoreCalculateSobelMatrix(WritableLockBitImage sourceImage, int[] firstKernel, int[] secondKernel)
        {
            // Go through each pixel in the source image
            int[] intermediateResult = new int[sourceImage.Height * sourceImage.Width];
            int[] inputBuffer = new int[3];
            int width = sourceImage.Width;
            for (int row = 0; row < sourceImage.Height; row++)
            {
                for (int col = 0; col < sourceImage.Width; col++)
                {
                    // Set the input buffer
                    inputBuffer[0] = col > 0 ? sourceImage.GetPixel(col - 1, row).R : 0;
                    inputBuffer[1] = sourceImage.GetPixel(col, row).R;
                    inputBuffer[2] = col < sourceImage.Width - 1 ? sourceImage.GetPixel(col + 1, row).R : 0;

                    // Convolute it with the vector
                    intermediateResult[row * width + col] = ConvoluteOneDimensionalVector(inputBuffer, firstKernel);
                }
            }

            // Go through each result in the intermediate result
            int[] finalMatrix = new int[sourceImage.Height * sourceImage.Width];
            for (int row = 0; row < sourceImage.Height; row++)
            {
                for (int col = 0; col < sourceImage.Width; col++)
                {
                    // Set input buffer
                    inputBuffer[0] = row > 0 ? intermediateResult[((row - 1) * width) + col] : 0;
                    inputBuffer[1] = intermediateResult[row * width + col];
                    inputBuffer[2] = row < sourceImage.Height - 1 ? intermediateResult[((row + 1) * width) + col] : 0;

                    // Convolute it with the second vector
                    finalMatrix[row * width + col] = ConvoluteOneDimensionalVector(inputBuffer, secondKernel);
                }
            }

            return finalMatrix;
        }

        /// <summary>
        /// Convolutes the two 1-dimensional vectors, assuming that the kernel matrix is centered on top of the input matrix
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        private static int ConvoluteOneDimensionalVector(int[] inputMatrix, int[] kernel)
        {
            if (inputMatrix.Length != kernel.Length)
            {
                throw new ArgumentException("Dimensions of input matrix and kernel matrix are not equal");
            }

            int acc = 0;
            for (int pixel = 0; pixel < inputMatrix.Length; pixel++)
            {
                acc += inputMatrix[pixel] * kernel[kernel.Length - pixel - 1];
            }

            return acc;
        }
        #endregion
    }
}