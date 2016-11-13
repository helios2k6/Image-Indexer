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
using System.IO;
using VideoIndexer.Media;
using VideoIndexer.Y4M;

namespace VideoIndexer.Video
{
    internal static class Indexer
    {
        #region private static fields
        private static readonly TimeSpan PlaybackDuration = TimeSpan.FromSeconds(180);
        #endregion

        #region public methods
        public static IEnumerable<VideoFingerPrintWrapper> IndexVideo(string videoFile)
        {
            MediaInfo info = new MediaInfoProcess(videoFile).Execute();
            IndexEntries(videoFile, info);
        }
        #endregion

        #region private methods
        private static void IndexEntries(string videoFile, MediaInfo info)
        {
            TimeSpan totalDuration = info.GetDuration();
            for (var startTime = TimeSpan.FromSeconds(0); startTime < totalDuration; startTime += PlaybackDuration)
            {
                IndexEntriesAtIndex(videoFile, startTime, info.GetFramerate(), totalDuration);
            }
        }

        private static void IndexEntriesAtIndex(
            string videoFile,
            TimeSpan startTime,
            Ratio framerate,
            TimeSpan totalDuration
        )
        {
            string outputDirectory = Path.GetRandomFileName();
            Ratio quarterFramerate = new Ratio(framerate.Numerator, framerate.Denominator * 4);
            var ffmpegProcessSettings = new FFMPEGProcessSettings(
                videoFile,
                outputDirectory,
                startTime,
                CalculateFramesToOutputFromFramerate(startTime, quarterFramerate, totalDuration),
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
                IEnumerable<FrameFingerPrintWrapper> fingerPrints = IndexFilesInDirectory(videoFile, outputDirectory, startTime, quarterFramerate);
                try
                {
                    Directory.Delete(outputDirectory, true);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(string.Format("Could not clean up images: {0}", e.Message));
                }
            }
        }

        private static IEnumerable<FrameFingerPrintWrapper> IndexFilesInDirectory(string originalFileName, string directory, TimeSpan startTime, Ratio frameRate)
        {
            foreach (string file in Directory.EnumerateFiles(directory, "*.y4m"))
            {
                using (var parser = new VideoFileParser(file))
                {
                    Maybe<VideoFile> videoFileMaybe = parser.TryParseVideoFile();
                    if (videoFileMaybe.IsNothing())
                    {
                        return;
                    }

                    VideoFile videoFile = videoFileMaybe.Value;
                    int frameNumber = 0;
                    foreach (VideoFrame frame in videoFile.Frames)
                    {
                        // TODO: call fingerprinter

                        frameNumber++;
                    }
                }
            }
        }

        private static TimeSpan CalculateEndTime(TimeSpan startTime, Ratio frameRate, long frameNumber)
        {
            return startTime + TimeSpan.FromSeconds((frameRate.Denominator / (double)frameRate.Numerator) * frameNumber);
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