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
    internal sealed class DCTGreyScaleCalculator : IDisposable
    {
        #region private fields
        private bool _disposed;
        private readonly Image _sourceImage;
        private readonly int _length;
        #endregion

        #region public properties
        public DCTGreyScaleCalculator(Image sourceImage)
        {
            if (sourceImage.Width != sourceImage.Height)
            {
                throw new ArgumentException("Image must be square for DCT transform");
            }
            _disposed = false;
            _sourceImage = sourceImage.Clone() as Image;
            _length = _sourceImage.Width;
        }
        #endregion

        #region ctor
        #endregion

        #region public methods
        public static double[,] Calculate(Image sourceImage)
        {
            using (var calculator = new DCTGreyScaleCalculator(sourceImage))
            {
                return calculator.Calculate();
            }
        }

        public double[,] Calculate()
        {
            using (var sourceLockbitImage = new WritableLockBitImage(_sourceImage))
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
                                runningDctSum += sourceLockbitImage.GetPixel(xInputIndex, yInputIndex).R *
                                    CalculateDCTCoeff(
                                        sourceLockbitImage.GetPixel(xInputIndex, yInputIndex).R,
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
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _sourceImage.Dispose();
        }
        #endregion

        #region private methods
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
