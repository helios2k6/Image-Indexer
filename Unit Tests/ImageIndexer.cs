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

using ImageIndexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace UnitTests
{
    /// <summary>
    /// Summary description for ImageIndexer
    /// </summary>
    [TestClass]
    public class ImageIndexer
    {
        private static readonly string TestPhoto1 = "UnitTests.TestData.TEST_PHOTO_1.png";
        private static readonly string TestPhoto2 = "UnitTests.TestData.TEST_PHOTO_2.png";
        private static readonly string TestPhoto3 = "UnitTests.TestData.TEST_PHOTO_3.png";

        [TestMethod]
        public void SamePhotoTest()
        {
            using (Image testImage = GetTestImage(TestPhoto1))
            {
                VideoFingerPrintWrapper fingerprint = Indexer.IndexVideo(new[] { testImage }, string.Empty);
                VideoFingerPrintWrapper fingerPrint2 = Indexer.IndexVideo(new[] { testImage }, string.Empty);

                Assert.AreEqual(fingerPrint2, fingerprint);
            }
        }

        [TestMethod]
        public void VerySimilarPhotoTest()
        {
            using (Image testImage1 = GetTestImage(TestPhoto1))
            using (Image testImage2 = GetTestImage(TestPhoto2))
            {
                VideoFingerPrintWrapper fingerprint1 = Indexer.IndexVideo(new[] { testImage1 }, string.Empty);
                VideoFingerPrintWrapper fingerPrint2 = Indexer.IndexVideo(new[] { testImage2 }, string.Empty);

                int distance = DistanceCalculator.CalculateDistance(fingerprint1.FingerPrints[0], fingerPrint2.FingerPrints[0]);

                Assert.AreEqual(2, distance);
            }
        }

        [TestMethod]
        public void TotallyDifferentPhotosTest()
        {
            using (Image testImage1 = GetTestImage(TestPhoto1))
            using (Image testImage2 = GetTestImage(TestPhoto3))
            {
                VideoFingerPrintWrapper fingerprint1 = Indexer.IndexVideo(new[] { testImage1 }, string.Empty);
                VideoFingerPrintWrapper fingerPrint2 = Indexer.IndexVideo(new[] { testImage2 }, string.Empty);

                int distance = DistanceCalculator.CalculateDistance(fingerprint1.FingerPrints[0], fingerPrint2.FingerPrints[0]);

                Assert.AreEqual(7, distance);
            }
        }

        private static Image GetTestImage(string path)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            {
                return new Bitmap(stream);
            }
        }
    }
}
