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

using Core.Compression;
using System.IO;

namespace Core.Model.Serialization
{
    /// <summary>
    /// Utility methods for serialization
    /// </summary>
    internal static class SerializationUtils
    {
        /// <summary>
        /// Compress an array of bytes that represent the gray scale thumbnail
        /// </summary>
        /// <param name="inputGrayScaleThumb"></param>
        /// <returns></returns>
        public static byte[] CompressedGrayScaleThumb(byte[] inputGrayScaleThumb)
        {
            using (var outputStream = new MemoryStream())
            using (var arithmeticStream = new ArithmeticStream(outputStream, CompressionMode.Compress, true))
            {
                arithmeticStream.Write(inputGrayScaleThumb, 0, inputGrayScaleThumb.Length);
                arithmeticStream.Flush();
                arithmeticStream.Close();
                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress an array of bytes that represent the gray scale thumbnail
        /// </summary>
        /// <param name="compressedGrayScaleThumb"></param>
        /// <returns></returns>
        public static byte[] DecompressGrayScaleThumb(byte[] compressedGrayScaleThumb)
        {
            using (var inputStream = new MemoryStream(compressedGrayScaleThumb))
            using (var arithmeticStream = new ArithmeticStream(inputStream, CompressionMode.Decompress, true))
            using (var outputStream = new MemoryStream())
            {
                arithmeticStream.CopyTo(outputStream);
                arithmeticStream.Flush();
                outputStream.Flush();
                outputStream.Close();
                return outputStream.ToArray();
            }
        }
    }
}