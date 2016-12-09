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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoIndexer.Media;
using VideoIndexer.Wrappers;
using VideoIndexer.BGR24;
using System.Collections.Concurrent;

namespace VideoIndexer.Video
{
    internal static class VideoIndexer
    {
        #region private classes
        private class WorkItem
        {
            public int NumFrames { get; set; }
            public int CurrentFrameIndex { get; set; }
        }
        #endregion

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
            var fingerPrints = new ConcurrentBag<FrameFingerPrintWrapper>();
            TimeSpan totalDuration = info.GetDuration();
            Ratio framerate = info.GetFramerate();
            var quarterFramerate = new Ratio(framerate.Numerator, framerate.Denominator * 4);
            var blockingCollection = new BlockingCollection<WorkItem>(1); // Maximum of 1. These are the number of "spare" video files that we want waiting for the consumer thread
            var rawVideoPathCollection = new BlockingCollection<string>(1);

            // Producer Thread
            Task producer = Task.Factory.StartNew(() =>
            {
                int frameIndex = 0;
                for (var startTime = TimeSpan.FromSeconds(0); startTime < totalDuration; startTime += PlaybackDuration)
                {
                    string outputDirectory = Path.GetRandomFileName();
                    int numFramesToOutput = CalculateFramesToOutputFromFramerate(startTime, quarterFramerate, totalDuration);
                    var ffmpegProcessSettings = new FFMPEGProcessSettings(
                        videoFile,
                        outputDirectory,
                        startTime,
                        numFramesToOutput,
                        quarterFramerate,
                        FFMPEGOutputFormat.GBR24
                    );

                    if (Directory.Exists(outputDirectory) == false)
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    using (var ffmpegProcess = new FFMPEGProcess(ffmpegProcessSettings))
                    {
                        blockingCollection.Add(new WorkItem
                        {
                            CurrentFrameIndex = frameIndex,
                            NumFrames = numFramesToOutput,
                        });

                        ffmpegProcess.Execute();

                        rawVideoPathCollection.Add(ffmpegProcess.GetOutputFilePath());

                        frameIndex += numFramesToOutput;
                    }
                }

                blockingCollection.CompleteAdding();
            });

            // Consumer Thread
            Task consumer = Task.Factory.StartNew(() =>
            {
                foreach (WorkItem workItem in blockingCollection.GetConsumingEnumerable())
                {
                    string rawVideoPath = rawVideoPathCollection.Take();
                    IEnumerable<FrameFingerPrintWrapper> videoSegmentFingerPrints = IndexVideoSegment(
                        rawVideoPath,
                        workItem.NumFrames,
                        workItem.CurrentFrameIndex,
                        info
                    );

                    try
                    {
                        Directory.Delete(Path.GetDirectoryName(rawVideoPath), true);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(string.Format("Could not clean up output folder: {0}", e.Message));
                    }

                    foreach (var frameFingerPrint in videoSegmentFingerPrints)
                    {
                        fingerPrints.Add(frameFingerPrint);
                    }
                }
            });

            Task.WaitAll(producer, consumer);

            return fingerPrints;
        }

        private static IEnumerable<FrameFingerPrintWrapper> IndexVideoSegment(
            string rawVideoFilePath,
            int numFramesToOutput,
            int currentFrameIndex,
            MediaInfo info
        )
        {
            return IndexFilesInDirectoryUsingBGR24(
                rawVideoFilePath,
                currentFrameIndex,
                info.GetWidth(),
                info.GetHeight(),
                numFramesToOutput
            );
        }

        private static IEnumerable<FrameFingerPrintWrapper> IndexFilesInDirectoryUsingBGR24(
            string pathToOutputFile,
            int currentFrameIndex,
            int width,
            int height,
            int numFrames
        )
        {
            var reader = new BGR24VideoReader(pathToOutputFile, width, height, numFrames);
            VideoIndexingExecutor indexingPool = new VideoIndexingExecutor();
            Task producingTask = Task.Factory.StartNew(() =>
            {
                VideoDecodingPool.StartDecoding(reader, indexingPool, currentFrameIndex);
            });

            Task continuedTask = producingTask.ContinueWith(t =>
            {
                indexingPool.Shutdown();
                indexingPool.Wait();
            });

            continuedTask.Wait();

            return indexingPool.GetFingerPrints();
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