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

namespace VideoIndexer
{
    /// <summary>
    /// Represents the different formats that the FFMPEG process can output
    /// </summary>
    internal enum FFMPEGOutputFormat
    {
        /// <summary>
        /// Specifies the PNG image format
        /// </summary>
        PNG,
        /// <summary>
        /// Specifies the raw video Y4M output
        /// </summary>
        Y4M,
        /// <summary>
        /// Specifies the raw RGB24 output format
        /// </summary>
        RGB24,
        /// <summary>
        /// Specifies the raw BGR24 output format (which is a reversed RGB24 format)
        /// </summary>
        GBR24,
    }
}