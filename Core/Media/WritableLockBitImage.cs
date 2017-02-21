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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Core.Media
{
    /// <summary>
    /// An Image object that allows you to read from it form multiple threads
    /// </summary>
    public sealed class WritableLockBitImage : IDisposable
    {
        #region private fields
        private bool _disposed;
        private bool _locked;

        private readonly Bitmap _bitmap;
        private readonly BitmapData _bitmapData;
        private readonly int _bitDepth;
        private readonly int _width;
        private readonly int _height;
        #endregion

        #region public properties
        /// <summary>
        /// Get whether this lockbit image is locked
        /// </summary>
        public bool Locked
        {
            get { return _locked; }
        }

        /// <summary>
        /// The width of the image
        /// </summary>
        public int Width
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("Object already disposed");
                }
                return _width;
            }
        }

        /// <summary>
        /// The height of the image
        /// </summary>
        public int Height
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("Object already disposed");
                }
                return _height;
            }
        }

        /// <summary>
        /// Gets the amount of memory this image takes up
        /// </summary>
        public long MemorySize
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("Object already disposed");
                }

                long bytesPerBit = (_bitDepth / 8);
                long numberOfPixelsAndPadding = ((long)_bitmapData.Stride) * (Height);
                return bytesPerBit * numberOfPixelsAndPadding;
            }
        }
        #endregion

        #region ctor
        /// <summary>
        /// Creates a new writable lockbit image
        /// </summary>
        /// <remarks>
        /// This will not hold on to the original reference of the passed in image,
        /// so it's safe to dispose of any references passed to this object
        /// </remarks>
        public WritableLockBitImage(Image image)
            : this(image, true)
        {
        }

        /// <summary>
        /// Creates a new writable lockbit image, but gives the consumer the ability to pass a flag 
        /// specifying whether to clone the input image
        /// </summary>
        /// <param name="image">The image to use for this writable lockbit image</param>
        /// <param name="shouldClone">Whether or not to clone this image</param>
        public WritableLockBitImage(Image image, bool shouldClone)
        {
            _width = image.Width;
            _height = image.Height;
            _disposed = _locked = false;

            if (shouldClone)
            {
                _bitmap = new Bitmap(image.Clone() as Image);
            }
            else
            {
                _bitmap = new Bitmap(image);
            }

            _bitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite,
                image.PixelFormat
            );

            _bitDepth = Image.GetPixelFormatSize(image.PixelFormat);
            if (_bitDepth != 8 && _bitDepth != 24 && _bitDepth != 32)
            {
                throw new ArgumentException("Only 8, 24, and 32 bit pixels are supported.");
            }
        }

        /// <summary>
        /// Creates an empty writable lockbit image
        /// </summary>
        public WritableLockBitImage(int width, int height)
        {
            _width = width;
            _height = height;
            _disposed = _locked = false;
            _bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _bitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                _bitmap.PixelFormat
            );
            _bitDepth = 24;
        }

        /// <summary>
        /// Creats a writable lockbit image with the given frame byte array
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="frame"></param>
        public WritableLockBitImage(int width, int height, byte[] frame)
        {
            _width = width;
            _height = height;
            _disposed = _locked = false;
            _bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _bitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                _bitmap.PixelFormat
            );

            _bitDepth = 24;

            Marshal.Copy(frame, 0, _bitmapData.Scan0, frame.Length);
        }
        #endregion

        #region public methods
        /// <summary>
        /// Get the pixel at a specific point in this image
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <returns>A color representing this pixel</returns>
        public Color GetPixel(int x, int y)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Object already disposed");
            }

            if (x > Width || y > Height || x < 0 || y < 0)
            {
                throw new IndexOutOfRangeException();
            }

            unsafe
            {
                // Get color components count
                int cCount = _bitDepth / 8;
                Color clr = Color.Empty;
                byte* currentLine = ((byte*)_bitmapData.Scan0) + (y * _bitmapData.Stride);
                int offsetInBytes = x * cCount;
                if (_bitDepth == 32) // For 32 bpp get Red, Green, Blue and Alpha
                {
                    byte b = *(currentLine + offsetInBytes);
                    byte g = *(currentLine + offsetInBytes + 1);
                    byte r = *(currentLine + offsetInBytes + 2);
                    byte a = *(currentLine + offsetInBytes + 3);
                    clr = Color.FromArgb(a, r, g, b);
                }
                else if (_bitDepth == 24) // For 24 bpp get Red, Green and Blue
                {
                    byte b = *(currentLine + offsetInBytes);
                    byte g = *(currentLine + offsetInBytes + 1);
                    byte r = *(currentLine + offsetInBytes + 2);
                    clr = Color.FromArgb(r, g, b);
                }
                else if (_bitDepth == 8)  // For 8 bpp get color value (Red, Green and Blue values are the same)
                {
                    byte b = *(currentLine + offsetInBytes);
                    clr = Color.FromArgb(b, b, b);
                }
                return clr;
            }
        }

        /// <summary>
        /// Sets the pixel color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            if (_locked)
            {
                throw new InvalidOperationException("Cannot modify a locked image");
            }

            if (_disposed)
            {
                throw new ObjectDisposedException("Object already disposed");
            }

            if (x > Width || y > Height || x < 0 || y < 0)
            {
                throw new IndexOutOfRangeException();
            }

            unsafe
            {
                // Get color components count
                int cCount = _bitDepth / 8;
                byte* currentLine = ((byte*)_bitmapData.Scan0) + (y * _bitmapData.Stride);
                int offsetInBytes = x * cCount;
                if (_bitDepth == 32) // For 32 bpp get Red, Green, Blue and Alpha
                {
                    *(currentLine + offsetInBytes) = color.B;
                    *(currentLine + offsetInBytes + 1) = color.G;
                    *(currentLine + offsetInBytes + 2) = color.R;
                    *(currentLine + offsetInBytes + 3) = color.A;
                }
                else if (_bitDepth == 24) // For 24 bpp get Red, Green and Blue
                {
                    *(currentLine + offsetInBytes) = color.B;
                    *(currentLine + offsetInBytes + 1) = color.G;
                    *(currentLine + offsetInBytes + 2) = color.R;
                }
                else if (_bitDepth == 8)  // For 8 bpp get color value (Red, Green and Blue values are the same)
                {
                    *(currentLine + offsetInBytes) = color.B;
                }
            }
        }

        /// <summary>
        /// Sets the entire frame using raw bytes provided
        /// </summary>
        /// <param name="frameBuffer"></param>
        public void SetFrame(byte[] frameBuffer)
        {
            if (_locked)
            {
                throw new InvalidOperationException("Cannote modify a locked image");
            }

            if (_disposed)
            {
                throw new ObjectDisposedException("Object already disposed");
            }

            if (_width * _height * (_bitDepth / 8) != frameBuffer.Length)
            {
                throw new InvalidOperationException("Frame buffer does not match length of image");
            }

            Marshal.Copy(frameBuffer, 0, _bitmapData.Scan0, frameBuffer.Length);
        }

        /// <summary>
        /// Lock this image
        /// </summary>
        public void Lock()
        {
            if (_locked)
            {
                return;
            }
            _locked = true;
            _bitmap.UnlockBits(_bitmapData);
        }

        /// <summary>
        /// Get this WritableLockBitImage as an Image object
        /// </summary>
        /// <returns></returns>
        public Image GetImage()
        {
            if (_locked == false)
            {
                throw new InvalidOperationException("Cannot retrieve unlocked object");
            }

            return _bitmap;
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

            if (_locked == false)
            {
                Lock();
            }

            _locked = true;
            _disposed = true;

            _bitmap.Dispose();
        }
        #endregion
    }
}
