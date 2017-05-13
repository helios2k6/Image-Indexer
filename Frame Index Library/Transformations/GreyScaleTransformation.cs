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
    /// A transformation that turns an image into a grey scale
    /// </summary>
    internal static class GreyScaleTransformation
    {
        #region public methods
        /// <summary>
        /// Transform this image to a greyscale version in-place
        /// </summary>
        /// <remarks>This will transform the image INPLACE</remarks>
        /// <returns>A black and white image. This reference is equal to the input</returns>
        public static WritableLockBitImage TransformInPlace(WritableLockBitImage image)
        {
            Transform(image, image);
            return image;
        }

        /// <summary>
        /// Transform this image to a greyscale version out-of-place
        /// </summary>
        /// <param name="source">The source image</param>
        /// <param name="output">The image to write to</param>
        public static void Transform(WritableLockBitImage source, WritableLockBitImage output)
        {
            if (source.Locked || output.Locked)
            {
                throw new ArgumentException("Lockbit image is locked.");
            }

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color sourcePixel = source.GetPixel(x, y);

                    int greyColor =
                        (int)Math.Floor(sourcePixel.R * 0.299) +
                        (int)Math.Floor(sourcePixel.G * 0.587) +
                        (int)Math.Floor(sourcePixel.B * 0.114);

                    Color greyScale = Color.FromArgb(
                        greyColor,
                        greyColor,
                        greyColor
                    );
                    output.SetPixel(x, y, greyScale);
                }
            }
        }
        #endregion
    }
}
