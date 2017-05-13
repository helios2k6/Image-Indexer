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
using Core.Model.Utils;
using Core.Model.Wrappers;
using System.Collections.Generic;

namespace Core.Metrics
{
    /// <summary>
    /// Model to Metric Space utility class
    /// </summary>
    public static class ModelMetricUtils
    {
        /// <summary>
        /// Translate a metatable into a BKTree for easier querying
        /// </summary>
        /// <param name="metatable"></param>
        /// <returns></returns>
        public static BKTree<FrameMetricWrapper> CreateBKTree(VideoFingerPrintDatabaseMetaTableWrapper metatable)
        {
            var tree = new BKTree<FrameMetricWrapper>();
            foreach (VideoFingerPrintWrapper video in MetaTableUtils.EnumerateVideoFingerPrints(metatable))
            {
                foreach (FrameFingerPrintWrapper frame in video.FingerPrints)
                {
                    tree.Add(new FrameMetricWrapper
                    {
                        Frame = frame,
                        Video = video,
                    });
                }
            }

            return tree;
        }

        /// <summary>
        /// Collapses the set of results from a BKTree that have the same source video file
        /// </summary>
        /// <param name="treeResults"></param>
        /// <returns></returns>
        public static IDictionary<string, ISet<FrameMetricWrapper>> CollapseTreeResults(IDictionary<FrameMetricWrapper, int> treeResults)
        {
            var videoGroups = new Dictionary<string, ISet<FrameMetricWrapper>>();
            foreach (KeyValuePair<FrameMetricWrapper, int> entry in treeResults)
            {
                ISet<FrameMetricWrapper> bucket;
                if (videoGroups.TryGetValue(entry.Key.Video.FilePath, out bucket) == false)
                {
                    bucket = new HashSet<FrameMetricWrapper>();
                    videoGroups.Add(entry.Key.Video.FilePath, bucket);
                }

                bucket.Add(entry.Key);
            }

            return videoGroups;
        }
    }
}
