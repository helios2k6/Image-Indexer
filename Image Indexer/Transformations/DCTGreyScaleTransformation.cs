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
    internal sealed class DCTGreyScaleTransformation : ITransformation, IDisposable
    {
        #region private fields
        private bool _disposed;
        private readonly Image _sourceImage;
        #endregion

        #region public properties
        public DCTGreyScaleTransformation(Image sourceImage)
        {
            if (sourceImage.Width != sourceImage.Height)
            {
                throw new ArgumentException("Image must be square for DCT transform");
            }
            _disposed = false;
            _sourceImage = sourceImage.Clone() as Image;
        }
        #endregion

        #region ctor
        #endregion

        #region public methods
        public static Image Transform(Image sourceImage)
        {
            using (var transform = new DCTGreyScaleTransformation(sourceImage))
            {
                return transform.Transform();
            }
        }

        public Image Transform()
        {
            int width = _sourceImage.Width;
            int height = _sourceImage.Height;
            using (var sourceLockbitImage = new WritableLockBitImage(_sourceImage))
            using (var outputLockbitImage = new WritableLockBitImage(width, height))
            {
                for (int yOutputIndex = 0; yOutputIndex < height; yOutputIndex++)
                {
                    for (int xOutputIndex = 0; xOutputIndex < width; xOutputIndex++)
                    {
                        outputLockbitImage.SetPixel(xOutputIndex, yOutputIndex, Color.FromArgb(0, 0, 0));
                        for (int yInputIndex = 0; yInputIndex < height; yInputIndex++)
                        {
                            for (int xInputIndex = 0; xInputIndex < height; xInputIndex++)
                            {
                                Color currentOutputPixel = outputLockbitImage.GetPixel(xOutputIndex, yOutputIndex);
                                Color currentInputPixel = sourceLockbitImage.GetPixel(xInputIndex, yInputIndex);

                                // Only need to deal with the red channel because we're working with a greyscale image. 
                                int pixelOutput = (int)Math.Round(
                                    currentInputPixel.R * CalculateDCTCoeff(currentInputPixel.R, width, height, xInputIndex, yInputIndex, xOutputIndex, yOutputIndex)
                                );

                                outputLockbitImage.SetPixel(
                                    xOutputIndex,
                                    yOutputIndex,
                                    Color.FromArgb(
                                        currentOutputPixel.R + pixelOutput,
                                        0,
                                        0
                                    )
                                );
                            }
                        }
                    }
                }

                outputLockbitImage.Lock();
                return outputLockbitImage.GetImage();
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
        private static double CalculateDCTCoeff(
            int inputValue,
            int width,
            int height,
            int xInputIndex,
            int yInputIndex,
            int xOutputIndex,
            int yOutputIndex
        )
        {
            return Math.Cos((Math.PI / height) * (yInputIndex + 0.5) * yOutputIndex) *
                Math.Cos((Math.PI / width) * (xInputIndex + 0.5) * xOutputIndex);
        }
        #endregion
    }
}
