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

using Functional.Maybe;
using ImageIndexer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using VideoIndexer.Media;
using VideoIndexer.Y4M;

namespace VideoIndexer.Video
{
    internal static class VideoIndexer
    {
        #region private static fields
        private static readonly TimeSpan PlaybackDuration = TimeSpan.FromSeconds(180);
        #endregion

        #region public methods
        /// <summary>
        /// Index a video file by continuously dumping Y4M raw video and indexing it
        /// </summary>
        /// <param name="videoFile"></param>
        /// <returns></returns>
        public static VideoFingerPrintWrapper IndexVideo(string videoFile)
        {
            MediaInfo info = new MediaInfoProcess(videoFile).Execute();
            return new VideoFingerPrintWrapper
            {
                FilePath = videoFile,
                FingerPrints = IndexEntries(videoFile, info).ToArray(),
            };
        }
        #endregion

        #region private methods
        private static IEnumerable<FrameFingerPrintWrapper> IndexEntries(string videoFile, MediaInfo info)
        {
            var fingerPrints = new List<FrameFingerPrintWrapper>();
            TimeSpan totalDuration = info.GetDuration();
            Ratio framerate = info.GetFramerate();
            Ratio quarterFramerate = new Ratio(framerate.Numerator, framerate.Denominator * 4);
            int frameIndex = 0;
            for (var startTime = TimeSpan.FromSeconds(0); startTime < totalDuration; startTime += PlaybackDuration)
            {
                int numFramesToOutput = CalculateFramesToOutputFromFramerate(startTime, quarterFramerate, totalDuration);
                fingerPrints.AddRange(IndexVideoSegment(videoFile, startTime, framerate, numFramesToOutput, frameIndex));
                frameIndex += numFramesToOutput;
            }

            return fingerPrints;
        }

        private static IEnumerable<FrameFingerPrintWrapper> IndexVideoSegment(
            string videoFile,
            TimeSpan startTime,
            Ratio framerate,
            int numFramesToOutput,
            int currentFrameIndex
        )
        {
            string outputDirectory = Path.GetRandomFileName();
            var ffmpegProcessSettings = new FFMPEGProcessSettings(
                videoFile,
                outputDirectory,
                startTime,
                numFramesToOutput,
                framerate,
                FFMPEGOutputFormat.Y4M
            );

            if (Directory.Exists(outputDirectory) == false)
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (var ffmpegProcess = new FFMPEGProcess(ffmpegProcessSettings))
            {
                ffmpegProcess.Execute();
                IEnumerable<FrameFingerPrintWrapper> fingerPrints = IndexFilesInDirectory(
                    videoFile,
                    ffmpegProcess.GetOutputFilePath(),
                    currentFrameIndex
                );
                try
                {
                    Directory.Delete(outputDirectory, true);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(string.Format("Could not clean up output folder: {0}", e.Message));
                }
                return fingerPrints;
            }
        }

        private static IEnumerable<FrameFingerPrintWrapper> IndexFilesInDirectory(
            string originalFileName,
            string pathToOutputFile,
            int currentFrameIndex
        )
        {
            using (var parser = new VideoFileParser(pathToOutputFile))
            {
                Maybe<VideoFile> videoFileMaybe = parser.TryParseVideoFile();
                if (videoFileMaybe.IsNothing())
                {
                    return Enumerable.Empty<FrameFingerPrintWrapper>();
                }

                var fingerPrints = new List<FrameFingerPrintWrapper>();
                VideoFile videoFile = videoFileMaybe.Value;
                foreach (IImageFrame currentFrame in videoFile.Frames)
                {
                    using (Image convertedImage = ConvertToImage(currentFrame))
                    {
                        FrameFingerPrintWrapper fingerPrint = Indexer.IndexFrame(convertedImage, currentFrameIndex);
                        fingerPrints.Add(fingerPrint);
                    }

                    currentFrameIndex++;
                }

                return fingerPrints;
            }
        }

        private static Image ConvertToImage(IImageFrame frame)
        {
            using (var writableLockbitImage = new WritableLockBitImage(frame.Width, frame.Height))
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    for (int x = 0; x < frame.Width; x++)
                    {
                        writableLockbitImage.SetPixel(x, y, frame.GetPixel(x, y));
                    }
                }

                writableLockbitImage.Lock();
                return writableLockbitImage.GetImage();
            }
        }

        private static int CalculateFramesToOutputFromFramerate(TimeSpan index, Ratio framerate, TimeSpan totalDuration)
        {
            int numeratorMultiplier = index + PlaybackDuration < totalDuration
                ? (int)PlaybackDuration.TotalSeconds
                : (totalDuration - index).Seconds;

            return (framerate.Numerator * numeratorMultiplier) / framerate.Denominator;
        }
        #endregion
    }
}