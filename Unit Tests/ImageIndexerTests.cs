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

using Core.DSA;
using Core.Media;
using FrameIndexLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    /// <summary>
    /// Summary description for ImageIndexer
    /// </summary>
    [TestClass]
    public class ImageIndexerTests
    {
        [TestMethod]
        public void SamePhotoTest()
        {
            using (WritableLockBitImage testImage = TestUtils.GetImage(TestUtils.TestPhoto1))
            {
                ulong fingerPrint1 = FrameIndexer.IndexFrame(testImage);
                ulong fingerPrint2 = FrameIndexer.IndexFrame(testImage);

                Assert.AreEqual(fingerPrint1, fingerPrint2);
            }
        }

        [TestMethod]
        public void VerySimilarPhotoTest()
        {
            using (WritableLockBitImage testImage1 = TestUtils.GetImage(TestUtils.TestPhoto1))
            using (WritableLockBitImage testImage2 = TestUtils.GetImage(TestUtils.TestPhoto2))
            {
                ulong fingerPrint1 = FrameIndexer.IndexFrame(testImage1);
                ulong fingerPrint2 = FrameIndexer.IndexFrame(testImage2);
                int distance = DistanceCalculator.CalculateHammingDistance(fingerPrint1, fingerPrint2);

                Assert.AreEqual(3, distance);
            }
        }

        [TestMethod]
        public void TotallyDifferentPhotosTest()
        {
            using (WritableLockBitImage testImage1 = TestUtils.GetImage(TestUtils.TestPhoto1))
            using (WritableLockBitImage testImage2 = TestUtils.GetImage(TestUtils.TestPhoto3))
            {
                ulong fingerPrint1 = FrameIndexer.IndexFrame(testImage1);
                ulong fingerPrint2 = FrameIndexer.IndexFrame(testImage2);
                int distance = DistanceCalculator.CalculateHammingDistance(fingerPrint1, fingerPrint2);

                Assert.AreEqual(8, distance);
            }
        }
    }
}
