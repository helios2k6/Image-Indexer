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
        public static VideoFingerPrintWrapper IndexVideo(IEnumerable<WritableLockBitImage> frames, string filePath)
        {
            int frameNumber = 0;
            var frameFingerPrints = new List<FrameFingerPrintWrapper>();
            foreach (WritableLockBitImage frame in frames)
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

        /// <summary>
        /// Convienence method for indexing just the frames 
        /// </summary>
        /// <param name="frames">The frames to index</param>
        /// <returns>An IEnumerable of fingerprints</returns>
        public static IEnumerable<FrameFingerPrintWrapper> IndexFrames(IEnumerable<WritableLockBitImage> frames)
        {
            return IndexVideo(frames, string.Empty).FingerPrints;
        }

        /// <summary>
        /// Convienece method for indexing just 1 frame
        /// </summary>
        /// <param name="frame">The frame to index</param>
        /// <param name="frameNumber">The frame number of this frame</param>
        /// <returns>An indexed frame</returns>
        public static FrameFingerPrintWrapper IndexFrame(WritableLockBitImage frame, int frameNumber)
        {
            using (Image resizedImage = ResizeTransformation.Transform(frame, FingerPrintWidth, FingerPrintWidth))
            using (Image greyscalePixels = GreyScaleTransformation.Transform(resizedImage))
            {
                double[,] dctMatrix = FastDCTCalculator.Calculate(greyscalePixels);
                double averageGreyScaleValue = CalculateAverageOfDCT(dctMatrix);
                return new FrameFingerPrintWrapper
                {
                    FrameNumber = frameNumber,
                    PHashCode = ConstructHashCode(dctMatrix, averageGreyScaleValue),
                };
            }
        }
        #endregion

        #region private methods
        private static double CalculateAverageOfDCT(double[,] dctMatrix)
        {
            double runningSum = 0.0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    // Ignore the DC coefficient
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    runningSum += dctMatrix[y, x];
                }
            }

            return runningSum / 63.0;
        }

        private static ulong ConstructHashCode(double[,] dctMatrix, double averageGreyScaleValue)
        {
            ulong currentHashValue = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (dctMatrix[y, x] < averageGreyScaleValue)
                    {
                        ulong shiftedBit = ((ulong)1) << (x * y);
                        currentHashValue = currentHashValue | shiftedBit;
                    }
                }
            }

            return currentHashValue;
        }
        #endregion
    }
}
