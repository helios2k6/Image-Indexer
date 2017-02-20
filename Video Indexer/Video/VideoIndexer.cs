﻿/* 
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

using Core.Model.Wrappers;
using FFMPEGWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VideoIndexer.Media;

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
        public static VideoFingerPrintWrapper IndexVideo(string videoFile, CancellationToken cancellationToken, long maxMemory)
        {
            MediaInfo info = new MediaInfoProcess(videoFile).Execute();
            if (info.GetFramerate().Numerator == 0)
            {
                throw new InvalidOperationException("Invalid framerate for: " + videoFile);
            }

            return new VideoFingerPrintWrapper
            {
                FilePath = videoFile,
                FingerPrints = IndexVideo(videoFile, info, cancellationToken, maxMemory).ToArray(),
            };
        }
        #endregion

        #region private methods
        private static IEnumerable<FrameFingerPrintWrapper> IndexVideo(
            string videoFile,
            MediaInfo info,
            CancellationToken cancellationToken,
            long maxMemory
        )
        {
            TimeSpan totalDuration = info.GetDuration();
            var ffmpegProcessSettings = new FFMPEGProcessVideoSettings(
                videoFile,
                info.GetFramerate().Numerator,
                info.GetFramerate().Denominator * 4,
                FFMPEGMode.PlaybackAtFourX
            );

            using (var indexingPool = new VideoIndexingExecutor(4, cancellationToken, (long)Math.Round((3.0 * maxMemory) / 4.0)))
            using (var byteStore = new RawByteStore(info.GetWidth(), info.GetHeight(), indexingPool, cancellationToken, (long)Math.Round(maxMemory / 4.0)))
            using (var ffmpegProcess = new FFMPEGProcess(ffmpegProcessSettings, cancellationToken, (byteArray, bytesToSubmit) => { byteStore.Submit(byteArray, bytesToSubmit); }))
            {
                ffmpegProcess.Execute();

                byteStore.Shutdown();
                byteStore.Wait();
                indexingPool.Shutdown();
                indexingPool.Wait();

                return indexingPool.GetFingerPrints();
            }
        }
        #endregion
    }
}