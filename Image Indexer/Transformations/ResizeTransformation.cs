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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageIndexer
{
    /// <summary>
    /// A resizing transformation
    /// </summary>
    internal sealed class ResizeTransformation : ITransformation, IDisposable
    {
        private bool _disposed;
        private readonly Image _sourceImage;
        private readonly int _width;
        private readonly int _height;

        /// <summary>
        /// construct a new resize transformation
        /// </summary>
        /// <param name="sourceImage">The source image to transform</param>
        /// <param name="width">The width of the final image</param>
        /// <param name="height">The height of the final image</param>
        public ResizeTransformation(Image sourceImage, int width, int height)
        {
            _disposed = false;
            _sourceImage = sourceImage.Clone() as Image;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Transform the image
        /// </summary>
        /// <returns>A transformed image</returns>
        public Image Transform()
        {
            // Easy check to avoid lots of work for things already sized properly
            if (_width == _sourceImage.Width && _height == _sourceImage.Height)
            {
                return (_sourceImage.Clone() as Image);
            }

            var destRect = new Rectangle(0, 0, _width, _height);
            var destImage = new Bitmap(_width, _height);

            destImage.SetResolution(_sourceImage.HorizontalResolution, _sourceImage.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(_sourceImage, destRect, 0, 0, _sourceImage.Width, _sourceImage.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Factory version of this transformation
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image Transform(Image sourceImage, int width, int height)
        {
            using (var transformation = new ResizeTransformation(sourceImage, width, height))
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
    }
}
