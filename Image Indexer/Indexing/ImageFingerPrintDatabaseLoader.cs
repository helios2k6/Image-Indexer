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

using FlatBuffers;
using System.IO;
using System;
using System.Drawing;

namespace ImageIndexer
{
    /// <summary>
    /// Loads the image fingerprinting database
    /// </summary>
    internal static class ImageFingerPrintDatabaseLoader
    {
        #region private fields
        private static readonly int BufferSize = 4096; // 4 kibibytes
        #endregion

        #region public methods
        public static ImageFingerPrintDatabaseWrapper LoadDatabase(string path)
        {
            return Convert(LoadFlatBufferDatabase(path));
        }
        #endregion

        #region private methods
        /// <summary>
        /// Load the database
        /// </summary>
        /// <param name="path">The path to the binary file</param>
        /// <returns>A newly loaded database</returns>
        private static ImageFingerPrintDatabase LoadFlatBufferDatabase(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || File.Exists(path) == false)
            {
                throw new ArgumentException();
            }

            using (var memoryStream = new MemoryStream())
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                var buffer = new byte[BufferSize];
                int count = 0;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    memoryStream.Write(buffer, 0, count);
                }

                var byteBuffer = new ByteBuffer(memoryStream.ToArray());
                return ImageFingerPrintDatabase.GetRootAsImageFingerPrintDatabase(byteBuffer);
            }
        }

        private static ImageFingerPrintDatabaseWrapper Convert(ImageFingerPrintDatabase flatbuffer)
        {
            ImageFingerPrintWrapper[] fingerprints = new ImageFingerPrintWrapper[flatbuffer.FingerprintsLength];
            for (int i = 0; i < flatbuffer.FingerprintsLength; i++)
            {
                fingerprints[i] = Convert(flatbuffer.GetFingerprints(i));
            }

            return new ImageFingerPrintDatabaseWrapper(fingerprints);
        }

        private static ImageFingerPrintWrapper Convert(ImageFingerPrint fingerPrint)
        {
            MacroblockWrapper[] macroblocks = new MacroblockWrapper[fingerPrint.MacroblocksLength];
            for (int i = 0; i < fingerPrint.MacroblocksLength; i++)
            {
                macroblocks[i] = Convert(fingerPrint.GetMacroblocks(i));
            }

            return new ImageFingerPrintWrapper
            {
                FilePath = fingerPrint.FilePath,
                Macroblocks = macroblocks,
            };
        }

        private static MacroblockWrapper Convert(Macroblock macroblock)
        {
            Color[] pixels = new Color[macroblock.PixelsLength];
            for (int i = 0; i < macroblock.PixelsLength; i++)
            {
                pixels[i] = Convert(macroblock.GetPixels(i));
            }

            return new MacroblockWrapper
            {
                Pixels = pixels,
            };
        }

        private static Color Convert(Pixel pixel)
        {
            return Color.FromArgb(pixel.Red, pixel.Green, pixel.Blue);
        }
        #endregion
    }
}