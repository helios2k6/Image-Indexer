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

using Core.Console;
using Core.DSA;
using Core.Media;
using Core.Model.Serialization;
using Core.Model.Utils;
using Core.Model.Wrappers;
using FFMPEGWrapper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace ProduceNegativeExamples
{
    public static class Driver
    {
        #region private static fields
        private static readonly int DefaultMetricThreshold = 0;

        #endregion

        #region internal classes
        private sealed class FrameWrapper : IMetric<FrameFingerPrintWrapper>, IMetric<PhotoFingerPrintWrapper>, IMetric<FrameWrapper>
        {
            public FrameFingerPrintWrapper Frame { get; set; }

            public VideoFingerPrintWrapper Video { get; set; }

            public int CalculateDistance(FrameWrapper other)
            {
                return Frame.CalculateDistance(other.Frame);
            }

            public int CalculateDistance(PhotoFingerPrintWrapper other)
            {
                return Frame.CalculateDistance(other);
            }

            public int CalculateDistance(FrameFingerPrintWrapper other)
            {
                return Frame.CalculateDistance(other);
            }
        }

        private sealed class PhotoWrapper : IMetric<FrameWrapper>, IMetric<PhotoWrapper>
        {
            public PhotoFingerPrintWrapper Photo { get; set; }

            public int CalculateDistance(PhotoWrapper other)
            {
                return Photo.CalculateDistance(other.Photo);
            }

            public int CalculateDistance(FrameWrapper other)
            {
                return Photo.CalculateDistance(other.Frame);
            }
        }
        #endregion

        public static void Main(string[] args)
        {

            /* 
             * 1. Pull all images from the database
             * 2. Determine which anime they're all from
             * 3. Load all frames that are not that in the collection of pictures
             */
            var k = MapPhotosToVideos(
                LoadPhotoDatabase(args),
                LoadMetaTable(args)
            );
            Console.WriteLine(k);
        }

        private static PhotoFingerPrintDatabaseWrapper LoadPhotoDatabase(string[] args)
        {
            IEnumerable<string> databaseFilePath = ConsoleUtils.GetArgumentTuple(args, "--photo-database");
            if (databaseFilePath.Count() != 1)
            {
                throw new Exception("Photo database not provided or too many arguements provided");
            }

            return PhotoFingerPrintDatabaseLoader.Load(databaseFilePath.First());
        }

        private static VideoFingerPrintDatabaseMetaTableWrapper LoadMetaTable(string[] args)
        {
            IEnumerable<string> databaseFilePath = ConsoleUtils.GetArgumentTuple(args, "--video-metatable");
            if (databaseFilePath.Count() != 1)
            {
                throw new Exception("Video metatable not provided or too many arguements provided");
            }

            return VideoFingerPrintDatabaseMetaTableLoader.Load(databaseFilePath.First());
        }

        private static IDictionary<string, ISet<PhotoFingerPrintWrapper>> MapPhotosToVideos(
            PhotoFingerPrintDatabaseWrapper photoDatabase,
            VideoFingerPrintDatabaseMetaTableWrapper metatable
        )
        {
            IDictionary<string, ISet<PhotoFingerPrintWrapper>> resultMap = new Dictionary<string, ISet<PhotoFingerPrintWrapper>>();
            IDictionary<string, VideoFingerPrintWrapper> fileNameToVideoFingerPrintMap = MetaTableUtils.EnumerateVideoFingerPrints(metatable).ToDictionary(e => e.FilePath);
            BKTree<FrameWrapper> bktree = CreateBKTree(metatable);
            foreach (PhotoFingerPrintWrapper photo in photoDatabase.PhotoFingerPrints)
            {
                // 1. Find bucket of possible candidates
                IDictionary<FrameWrapper, int> treeResults = bktree.Query(
                    new PhotoWrapper
                    {
                        Photo = photo,
                    },
                    DefaultMetricThreshold
                );

                IDictionary<string, ISet<FrameWrapper>> collapsedTreeResults = CollapseTreeResults(treeResults);

                // 2. Find most likely result and add it to the bucket
                if (treeResults.Count > 0)
                {
                    VideoFingerPrintWrapper mostLikelyVideo = FindMostLikelyVideo(photo, collapsedTreeResults, fileNameToVideoFingerPrintMap);

                    // In the case where we didn't get any results, we just skip this photo and move alone
                    if (mostLikelyVideo == null)
                    {
                        continue;
                    }

                    ISet<PhotoFingerPrintWrapper> bucket;
                    string videoFileName = mostLikelyVideo.FilePath;
                    if (resultMap.TryGetValue(videoFileName, out bucket) == false)
                    {
                        bucket = new HashSet<PhotoFingerPrintWrapper>();
                        resultMap.Add(videoFileName, bucket);
                    }
                }
            }

            return resultMap;
        }

        /// <summary>
        /// Collapses results from the same video into one entry
        /// </summary>
        /// <param name="treeResults"></param>
        /// <returns></returns>
        private static IDictionary<string, ISet<FrameWrapper>> CollapseTreeResults(IDictionary<FrameWrapper, int> treeResults)
        {
            var videoGroups = new Dictionary<string, ISet<FrameWrapper>>();
            foreach (KeyValuePair<FrameWrapper, int> entry in treeResults)
            {
                ISet<FrameWrapper> bucket;
                if (videoGroups.TryGetValue(entry.Key.Video.FilePath, out bucket) == false)
                {
                    bucket = new HashSet<FrameWrapper>();
                    videoGroups.Add(entry.Key.Video.FilePath, bucket);
                }

                bucket.Add(entry.Key);
            }

            return videoGroups;
        }

        private static VideoFingerPrintWrapper FindMostLikelyVideo(
            PhotoFingerPrintWrapper photo,
            IDictionary<string, ISet<FrameWrapper>> treeResults,
            IDictionary<string, VideoFingerPrintWrapper> nameToVideoMap
        )
        {
            if (treeResults.Count() == 1)
            {
                return treeResults.First().Value.First().Video;
            }

            double currentBestSSIM = 0.0;
            VideoFingerPrintWrapper currentBestVideo = null;
            using (WritableLockBitImage photoAsLockBitImage = new WritableLockBitImage(Image.FromFile(photo.FilePath), false))
            {
                foreach (KeyValuePair<string, ISet<FrameWrapper>> videoFrameResults in treeResults)
                {
                    foreach (FrameWrapper frameWrapper in videoFrameResults.Value)
                    {
                        // Calculate SSIM
                        using (WritableLockBitImage videoFrame = GetFrameFromVideo(frameWrapper.Video, frameWrapper.Frame.FrameNumber))
                        {
                            double possibleBestSSIM = SSIMCalculator.Compute(photoAsLockBitImage, videoFrame);
                            // SSIM must be at least good enough for us to consider
                            if (possibleBestSSIM > currentBestSSIM)
                            {
                                currentBestSSIM = possibleBestSSIM;
                                currentBestVideo = frameWrapper.Video;
                            }
                        }
                    }
                }
            }


            return currentBestVideo;
        }

        private static WritableLockBitImage GetFrameFromVideo(VideoFingerPrintWrapper video, int frameNumber)
        {
            using (MediaInfoProcess mediaInfoProcess = new MediaInfoProcess(video.FilePath))
            {
                MediaInfo mediaInfo = mediaInfoProcess.Execute();
                if (mediaInfo.GetFramerate().Numerator == 0)
                {
                    throw new Exception("Did not get valid frame rate");
                }

                int width = mediaInfo.GetWidth();
                int height = mediaInfo.GetHeight();
                var ffmpegProcessSettings = new FFMPEGProcessVideoSettings(
                    video.FilePath,
                    mediaInfo.GetFramerate().Numerator,
                    mediaInfo.GetFramerate().Denominator * 4,
                    FFMPEGMode.SeekFrame
                );

                ffmpegProcessSettings.TargetFrame = frameNumber;
                byte[] frameBytes = new byte[width * height * 3];
                int offset = 0;
                using (
                    var ffmpegProcess = new FFMPEGProcess(
                        ffmpegProcessSettings,
                        CancellationToken.None,
                        (stdoutBytes, numBytes) =>
                        {
                            Buffer.BlockCopy(stdoutBytes, 0, frameBytes, offset, numBytes);
                            offset += numBytes;
                        },
                        false
                    )
                )
                {
                    ffmpegProcess.Execute();
                    if (offset != 3 * width * height)
                    {
                        throw new Exception("Did not get all bytes to produce valid frame");
                    }

                    WritableLockBitImage videoFrame = new WritableLockBitImage(width, height, frameBytes);
                    videoFrame.Lock();
                    return videoFrame;
                }
            }
        }

        private static BKTree<FrameWrapper> CreateBKTree(VideoFingerPrintDatabaseMetaTableWrapper metatable)
        {
            var tree = new BKTree<FrameWrapper>();
            foreach (VideoFingerPrintWrapper video in MetaTableUtils.EnumerateVideoFingerPrints(metatable))
            {
                foreach (FrameFingerPrintWrapper frame in video.FingerPrints)
                {
                    tree.Add(new FrameWrapper
                    {
                        Frame = frame,
                        Video = video,
                    });
                }
            }

            return tree;
        }
    }
}
