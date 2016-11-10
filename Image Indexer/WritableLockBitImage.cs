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
using System.Threading.Tasks;

namespace ImageIndexer
{
    /// <summary>
    /// An Image object that allows you to read from it form multiple threads
    /// </summary>
    public sealed class WritableLockBitImage : IDisposable, IImageFrame
    {
        private bool _disposed;
        private bool _locked;

        private readonly Bitmap _bitmap;
        private readonly BitmapData _bitmapData;
        private readonly byte[] _buffer;
        private readonly int _bitDepth;
        private readonly int _width;
        private readonly int _height;

        /// <summary>
        /// Creates a new writable lockbit image
        /// </summary>
        /// <remarks>
        /// This will not hold on to the original reference of the passed in image,
        /// so it's safe to dispose of any references passed to this object
        /// </remarks>
        public WritableLockBitImage(Image image)
        {
            _disposed = _locked = false;
            _bitmap = new Bitmap(image.Clone() as Image);
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
            _buffer = new byte[_bitmapData.Width * _bitmapData.Height * (_bitDepth / 8)];
            _width = image.Width;
            _height = image.Height;

            Marshal.Copy(_bitmapData.Scan0, _buffer, 0, _buffer.Length);
        }

        /// <summary>
        /// Creates an empty writable lockbit image
        /// </summary>
        public WritableLockBitImage(int width, int height)
        {
            _disposed = _locked = false;
            _bitmap = new Bitmap(width, height);
            _bitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                _bitmap.PixelFormat
            );

            _bitDepth = Image.GetPixelFormatSize(_bitmap.PixelFormat);
            if (_bitDepth != 8 && _bitDepth != 24 && _bitDepth != 32)
            {
                throw new ArgumentException("Only 8, 24, and 32 bit pixels are supported.");
            }
            _buffer = new byte[_bitmapData.Width * _bitmapData.Height * (_bitDepth / 8)];
            _width = width;
            _height = height;

            Marshal.Copy(_bitmapData.Scan0, _buffer, 0, _buffer.Length);
        }

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

            Color clr = Color.Empty;

            // Get color components count
            int cCount = _bitDepth / 8;

            // Get start index of the specified pixel
            int offset = ((y * Width) + x) * cCount;

            if (offset > _buffer.Length - cCount)
            {
                throw new IndexOutOfRangeException();
            }

            if (_bitDepth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = _buffer[offset];
                byte g = _buffer[offset + 1];
                byte r = _buffer[offset + 2];
                byte a = _buffer[offset + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (_bitDepth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = _buffer[offset];
                byte g = _buffer[offset + 1];
                byte r = _buffer[offset + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (_bitDepth == 8)  // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = _buffer[offset];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

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
            // Get color components count
            int cCount = _bitDepth / 8;

            // Get start index of the specified pixel
            int offset = ((y * Width) + x) * cCount;

            if (offset > _buffer.Length - cCount)
            {
                throw new IndexOutOfRangeException();
            }

            if (_bitDepth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                _buffer[offset] = color.B;
                _buffer[offset + 1] = color.G;
                _buffer[offset + 2] = color.R;
                _buffer[offset + 3] = color.A; // a
            }
            if (_bitDepth == 24) // For 24 bpp get Red, Green and Blue
            {
                _buffer[offset] = color.B;
                _buffer[offset + 1] = color.G;
                _buffer[offset + 2] = color.R;
            }
            if (_bitDepth == 8)  // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                _buffer[offset] = color.B;
            }
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

        public void Lock()
        {
            if (_locked)
            {
                return;
            }
            _locked = true;
            //Marshal.Copy(_buffer, 0, _bitmapData.Scan0, _buffer.Length);
            WriteBufferDirectlyToMemory();
            _bitmap.UnlockBits(_bitmapData);
        }

        public Image GetImage()
        {
            if (_locked == false)
            {
                throw new InvalidOperationException("Cannot retrieve unlocked object");
            }

            return _bitmap.Clone() as Image;
        }

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

        private void WriteBufferDirectlyToMemory()
        {
            unsafe
            {
                int bytesPerPixel = _bitDepth / 8;
                int widthInBytes = _bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)_bitmapData.Scan0;

                Parallel.For(0, Height, y =>
                {
                    byte* currentLine = ptrFirstPixel + (y * _bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        Color bufferColor = GetPixel(x / bytesPerPixel, y);
                        if (_bitDepth == 32) // For 32 bpp get Red, Green, Blue and Alpha
                        {
                            currentLine[x] = bufferColor.B;
                            currentLine[x + 1] = bufferColor.G;
                            currentLine[x + 2] = bufferColor.R;
                            currentLine[x + 3] = bufferColor.A;
                        }
                        if (_bitDepth == 24) // For 24 bpp get Red, Green and Blue
                        {
                            currentLine[x] = bufferColor.B;
                            currentLine[x + 1] = bufferColor.G;
                            currentLine[x + 2] = bufferColor.R;
                        }
                        if (_bitDepth == 8)  // For 8 bpp get color value (Red, Green and Blue values are the same)
                        {
                            currentLine[x] = bufferColor.B;
                        }
                    }
                });
            }
        }
    }
}
