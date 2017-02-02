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

using FrameIndexLibrary;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace UnitTests
{
    public static class TestUtils
    {
        public static readonly string TestPhoto1 = "UnitTests.TestData.TEST_PHOTO_1.png";
        public static readonly string TestPhoto2 = "UnitTests.TestData.TEST_PHOTO_2.png";
        public static readonly string TestPhoto3 = "UnitTests.TestData.TEST_PHOTO_3.png";

        /// <summary>
        /// Get a WritableLockBitImage that is automatically locked for you
        /// </summary>
        /// <param name="path">The resource path of the image</param>
        /// <returns>The locked writable lockbit image</returns>
        public static WritableLockBitImage GetImage(string path)
        {
            return GetImage(path, true);
        }

        /// <summary>
        /// Get a WritableLockBitImage that allows you to specify whether or not to lock the image
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lockImage"></param>
        /// <returns></returns>
        public static WritableLockBitImage GetImage(string path, bool lockImage)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            {
                var image = new WritableLockBitImage(new Bitmap(stream), false);
                if (lockImage)
                {
                    image.Lock();
                }
                return image;
            }
        }
    }
}
