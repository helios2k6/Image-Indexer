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
using Core.Model.Wrappers;
using System.Collections.Generic;
using Core.Model.Utils;
using System.Linq;
using System;

namespace ProduceNegativeExamples
{
    public static class Driver
    {
        #region private static fields
        private static readonly int DefaultMetricThreshold = 2;
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
        }

        private static PhotoFingerPrintDatabaseWrapper LoadPhotoDatabase(string[] args)
        {
            return null;
        }

        private static VideoFingerPrintDatabaseMetaTableWrapper LoadMetaTable(string[] args)
        {
            return null;
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

                // 2. If there's only 1 result, assume it's the source video
                if (treeResults.Count == 1)
                {
                    ISet<PhotoFingerPrintWrapper> bucket;
                    string videoFileName = treeResults.First().Key.Video.FilePath;
                    if (resultMap.TryGetValue(videoFileName, out bucket) == false)
                    {
                        bucket = new HashSet<PhotoFingerPrintWrapper>();
                        resultMap.Add(videoFileName, bucket);
                    }

                    bucket.Add(photo);
                }
                // 3. Otherwise, go through each possible video and run SSIM on it
                else if (treeResults.Count > 1)
                {

                }
            }

            return null;
        }

        private static VideoFingerPrintWrapper FindMostLikelyVideo(
            PhotoFingerPrintWrapper photo,
            IDictionary<FrameWrapper, int> treeResults,
            IDictionary<string, VideoFingerPrintWrapper> nameToVideoMap
        )
        {
            if (treeResults.Count < 1)
            {
                throw new ArgumentException("Empty tree results");
            }

            double currentSSIM = 0.0;
            VideoFingerPrintWrapper wrapperWithHighestSimilarity = null;
            foreach (KeyValuePair<FrameWrapper, int> result in treeResults)
            {
                // Calculate SSIM

            }

            return wrapperWithHighestSimilarity;
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
