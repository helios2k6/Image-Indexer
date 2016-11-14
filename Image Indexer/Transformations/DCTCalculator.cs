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
using System.Drawing;

namespace ImageIndexer
{
    /// <summary>
    /// Calculates the DCT of a given image or array of bytes
    /// </summary>
    public sealed class DCTCalculator
    {
        #region private fields
        private readonly byte[,] _sourceMatrix;
        private readonly int _length;
        #endregion

        #region ctor
        /// <summary>
        /// Construct a DCTCalculator
        /// </summary>
        /// <param name="sourceImage">The source image that has been greyscaled</param>
        public DCTCalculator(Image sourceImage)
        {
            if (sourceImage.Width != sourceImage.Height)
            {
                throw new ArgumentException("Image must be square for DCT transform");
            }
            _sourceMatrix = CopyImageToMatrix(sourceImage);
            _length = sourceImage.Width;
        }

        /// <summary>
        /// Construct a DCTCalculator
        /// </summary>
        /// <param name="sourceMatrix">The byte matrix to run a DCT on</param>
        public DCTCalculator(byte[,] sourceMatrix)
        {
            if (sourceMatrix.GetLength(0) != sourceMatrix.GetLength(1))
            {
                throw new ArgumentException("Matrix must be square");
            }

            _sourceMatrix = sourceMatrix;
            _length = _sourceMatrix.GetLength(0);
        }
        #endregion

        #region public methods
        /// <summary>
        /// Static factory version of this calculator
        /// </summary>
        /// <param name="sourceImage">The source image to transform</param>
        /// <returns>The DCT</returns>
        public static double[,] Calculate(Image sourceImage)
        {
            return new DCTCalculator(sourceImage).Calculate();
        }

        /// <summary>
        /// Static factory version of this calculator
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[,] Calculate(byte[,] source)
        {
            return new DCTCalculator(source).Calculate();
        }

        /// <summary>
        /// Calculate the DCT
        /// </summary>
        /// <returns>A matrix with the DCT coefficients</returns>
        public double[,] Calculate()
        {
            double[,] outputDCTMatrix = new double[_length, _length];
            for (int yOutputIndex = 0; yOutputIndex < _length; yOutputIndex++)
            {
                for (int xOutputIndex = 0; xOutputIndex < _length; xOutputIndex++)
                {
                    double runningDctSum = 0.0;
                    for (int yInputIndex = 0; yInputIndex < _length; yInputIndex++)
                    {
                        for (int xInputIndex = 0; xInputIndex < _length; xInputIndex++)
                        {
                            // Only need to deal with the red channel because we're working with a greyscale image.
                            byte pixelValue = _sourceMatrix[yInputIndex, xInputIndex];
                            runningDctSum += pixelValue *
                                CalculateDCTCoeff(
                                    pixelValue,
                                    _length,
                                    xInputIndex,
                                    yInputIndex,
                                    xOutputIndex,
                                    yOutputIndex
                                );
                        }
                    }

                    double normalizationCoeff = GetNormalizationCoefficient(xOutputIndex, yOutputIndex, _length);
                    outputDCTMatrix[yOutputIndex, xOutputIndex] = runningDctSum * normalizationCoeff;
                }
            }

            return outputDCTMatrix;
        }
        #endregion

        #region private methods
        private static byte[,] CopyImageToMatrix(Image image)
        {
            byte[,] sourceMatrix = new byte[image.Width, image.Height];
            using (var lockbitImage = new WritableLockBitImage(image))
            {
                for (int y = 0; y < lockbitImage.Height; y++)
                {
                    for (int x = 0; x < lockbitImage.Width; x++)
                    {
                        sourceMatrix[y, x] = lockbitImage.GetPixel(x, y).R;
                    }
                }
            }

            return sourceMatrix;
        }

        private static double GetNormalizationCoefficient(
            int xOutputIndex,
            int yOutputIndex,
            int length
        )
        {
            double alphaX = xOutputIndex == 0
                ? 1.0 / Math.Sqrt(length)
                : Math.Sqrt(2.0 / length);

            double alphaY = yOutputIndex == 0
                ? 1.0 / Math.Sqrt(length)
                : Math.Sqrt(2.0 / length);

            return alphaX * alphaY;
        }

        private static double CalculateDCTCoeff(
            int inputValue,
            int length,
            int xInputIndex,
            int yInputIndex,
            int xOutputIndex,
            int yOutputIndex
        )
        {
            return Math.Cos((Math.PI / length) * (yInputIndex + 0.5) * yOutputIndex) *
                Math.Cos((Math.PI / length) * (xInputIndex + 0.5) * xOutputIndex);
        }
        #endregion
    }
}
