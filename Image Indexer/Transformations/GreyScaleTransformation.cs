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
    /// A transformation that turns an image into a grey scale
    /// </summary>
    internal sealed class GreyScaleTransformation : ITransformation, IDisposable
    {
        #region private fields
        private bool _disposed;
        private readonly Image _sourceImage;
        #endregion

        #region ctor
        /// <summary>
        /// Construct this black and white transformation
        /// </summary>
        /// <param name="sourceImage"></param>
        public GreyScaleTransformation(Image sourceImage)
        {
            _disposed = false;
            _sourceImage = sourceImage.Clone() as Image;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Transform this image to black and white
        /// </summary>
        /// <returns>A black and white image</returns>
        public Image Transform()
        {
            using (var sourceLockbitImage = new WritableLockBitImage(_sourceImage))
            using (var outputLockbitImage = new WritableLockBitImage(_sourceImage.Width, _sourceImage.Height))
            {
                for (int y = 0; y < sourceLockbitImage.Height; y++)
                {
                    for (int x = 0; x < sourceLockbitImage.Width; x++)
                    {
                        Color sourcePixel = sourceLockbitImage.GetPixel(x, y);

                        int greyColor =
                            (int)Math.Floor(sourcePixel.R * 0.299) +
                            (int)Math.Floor(sourcePixel.G * 0.587) +
                            (int)Math.Floor(sourcePixel.B * 0.114);

                        Color greyScale = Color.FromArgb(
                            greyColor,
                            0,
                            0
                        );
                        outputLockbitImage.SetPixel(x, y, greyScale);
                    }
                }
                outputLockbitImage.Lock();
                return outputLockbitImage.GetImage();
            }
        }

        /// <summary>
        /// Factory version of this transformation
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <returns></returns>
        public static Image Transform(Image sourceImage)
        {
            using (var transformation = new GreyScaleTransformation(sourceImage))
            {
                return transformation.Transform();
            }
        }

        /// <summary>
        /// Dispose of this object
        /// </summary>
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
    }
}
