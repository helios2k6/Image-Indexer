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
using System.Drawing;

namespace ImageIndexer
{
    /// <summary>
    /// Indexes a video
    /// </summary>
    public static class Indexer
    {
        #region private fields
        private static readonly int FingerPrintWidth = 32;
        #endregion

        #region public methods
        /// <summary>
        /// Index a stream of frames
        /// </summary>
        /// <param name="frames">The frames to index</param>
        /// <param name="filePath">The path to the file that you are indexing</param>
        /// <returns>An indexed video</returns>
        public static VideoFingerPrintWrapper IndexVideo(IEnumerable<Image> frames, string filePath)
        {
            int frameNumber = 0;
            var frameFingerPrints = new List<FrameFingerPrintWrapper>();
            foreach (Image frame in frames)
            {
                frameFingerPrints.Add(IndexFrame(frame, frameNumber));
                frameNumber++;
            }

            return new VideoFingerPrintWrapper
            {
                FilePath = filePath,
                FingerPrints = frameFingerPrints.ToArray(),
            };
        }
        #endregion

        #region private methods
        private static FrameFingerPrintWrapper IndexFrame(Image frame, int frameNumber)
        {
            using (Image resizedImage = ResizeTransformation.Transform(frame, FingerPrintWidth, FingerPrintWidth))
            using (Image greyscalePixels = GreyScaleTransformation.Transform(resizedImage))
            using (Image dctImage = DCTGreyScaleTransformation.Transform(greyscalePixels))
            {
                double averageGreyScaleValue = CalculateAverageOfDCT(dctImage);
                return new FrameFingerPrintWrapper
                {
                    FrameNumber = frameNumber,
                    GreyscalePixels = GetGreyScalePixels(greyscalePixels),
                    PHashCode = ConstructHashCode(dctImage, averageGreyScaleValue),
                };
            }
        }

        private static double CalculateAverageOfDCT(Image dctImage)
        {
            using (var lockbitImage = new WritableLockBitImage(dctImage))
            {
                int runningSum = 0;
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        // Ignore the DC coefficient
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }
                        runningSum += lockbitImage.GetPixel(x, y).R;
                    }
                }

                return runningSum / 63.0;
            }
        }

        private static ulong ConstructHashCode(Image dctImage, double averageGreyScaleValue)
        {
            using (var lockbitImage = new WritableLockBitImage(dctImage))
            {
                ulong currentHashValue = 0;
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (lockbitImage.GetPixel(x, y).R < averageGreyScaleValue)
                        {
                            ulong shiftedBit = ((ulong)1) << (x * y);
                            currentHashValue = currentHashValue | shiftedBit;
                        }
                    }
                }

                return currentHashValue;
            }

        }

        private static int[] GetGreyScalePixels(Image greyScaleImage)
        {
            using (var lockbitImage = new WritableLockBitImage(greyScaleImage))
            {
                int[] greyscaleImage = new int[lockbitImage.Width * lockbitImage.Height];
                for (int y = 0; y < lockbitImage.Height; y++)
                {
                    for (int x = 0; x < lockbitImage.Width; x++)
                    {
                        greyscaleImage[(y * lockbitImage.Width) + x] = lockbitImage.GetPixel(x, y).R;
                    }
                }
                return greyscaleImage;
            }
        }
        #endregion
    }
}
