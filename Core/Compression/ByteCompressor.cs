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

using System.Collections.Generic;
using System.Linq;

namespace Core.Compression
{
    /// <summary>
    /// Utility class to compress arrays of bytes into a packed byte. This class
    /// assumes that the bytes are either 0 or 1, which can be packed into a single byte
    /// </summary>
    public static class ByteCompressor
    {
        /// <summary>
        /// Compress the input byte array into a packed byte array
        /// </summary>
        public static byte[] Compress(byte[] input)
        {
            var buffer = new List<byte>();
            byte currentByteIndex = 0;
            byte currentByte = 0;
            foreach (byte inputByte in input)
            {
                if (inputByte != 0)
                {
                    currentByte = (byte)(currentByte ^ (1 << currentByteIndex));
                }

                currentByteIndex++;

                if (currentByteIndex == 8)
                {
                    // Reset after the byte is full
                    buffer.Add(currentByte);
                    currentByte = 0;
                    currentByteIndex = 0;
                }
            }

            return buffer.ToArray();
        }
    }
}