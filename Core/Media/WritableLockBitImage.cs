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
                ThrowIfDisposed();
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
                ThrowIfDisposed();
                return _height;
            }
        }

        /// <summary>
        /// The horizontal resolution
        /// </summary>
        public float HorizontalResolution
        {
            get
            {
                ThrowIfDisposed();
                return _bitmap.HorizontalResolution;
            }
        }

        /// <summary>
        /// The vertical resolution
        /// </summary>
        public float VerticalResolution
        {
            get
            {
                ThrowIfDisposed();
                return _bitmap.VerticalResolution;
            }
        }

        /// <summary>
        /// Gets the amount of memory this image takes up
        /// </summary>
        public long MemorySize
        {
            get
            {
                ThrowIfDisposed();

                long bytesPerBit = (_bitDepth / 8);
                long numberOfPixelsAndPadding = ((long)_bitmapData.Stride) * (Height);
                return bytesPerBit * numberOfPixelsAndPadding;
            }
        }

        /// <summary>
        /// Get the pixel format of the image
        /// </summary>
        public PixelFormat PixelFormat
        {
            get
            {
                ThrowIfDisposed();
                return _bitmap.PixelFormat;
            }
        }
        #endregion

        #region ctor
        /// <summary>
        /// Copy constructor that clones the other WritableLockbitImage. This will clone all
        /// of the internal data from the passed in WritableLockBitImage, with the exception
        /// of whether it was locked.
        /// </summary>
        public WritableLockBitImage(WritableLockBitImage other)
        {
            _bitDepth = Image.GetPixelFormatSize(other.PixelFormat);
            if (_bitDepth != 8 && _bitDepth != 24 && _bitDepth != 32)
            {
                throw new ArgumentException("Only 8, 24, and 32 bit pixels are supported.");
            }
            _width = other.Width;
            _height = other.Height;
            _disposed = _locked = false;
            _bitmap = new Bitmap(Width, Height);
            _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadWrite,
                other.PixelFormat
            );

            // Copy over bitmap data manually
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    SetPixel(col, row, other.GetPixel(col, row));
                }
            }
        }

        /// <summary>
        /// Creates a new WritableLockBitImage from an existing Image. This will not hold onto the original
        /// Image reference, and will instead use the Bitmap copy constructor
        /// </summary>
        /// <param name="image">The image to use for this writable lockbit image</param>
        public WritableLockBitImage(Image image)
        {
            _bitDepth = Image.GetPixelFormatSize(image.PixelFormat);
            if (_bitDepth != 8 && _bitDepth != 24 && _bitDepth != 32)
            {
                throw new ArgumentException("Only 8, 24, and 32 bit pixels are supported.");
            }
            _width = image.Width;
            _height = image.Height;
            _disposed = _locked = false;
            _bitmap = new Bitmap(image);
            _bitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite,
                image.PixelFormat
            );
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
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <param name="frame">The raw frame data, which is expected to be 24-bits, laid out as RGB</param>
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
            ThrowIfLocked();
            ThrowIfDisposed();

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
            ThrowIfLocked();
            ThrowIfDisposed();

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
            ThrowIfLocked();
            ThrowIfDisposed();

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

        #region private methods
        private void ThrowIfLocked()
        {
            if (_locked)
            {
                throw new InvalidOperationException("Object is locked");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Object is already disposed");
            }
        }
        #endregion
    }
}
