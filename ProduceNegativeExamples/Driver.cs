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

using Core.Model.Serialization;
using Core.Model.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProduceNegativeExamples
{
    public static class Driver
    {
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

        private static IDictionary<string, IEnumerable<PhotoFingerPrintWrapper>> MapPhotosToVideos(
            PhotoFingerPrintDatabaseWrapper photoDatabase,
            VideoFingerPrintDatabaseMetaTableWrapper metatable
        )
        {
            IDictionary<string, IEnumerable<PhotoFingerPrintWrapper>> resultMap = new Dictionary<string, IEnumerable<PhotoFingerPrintWrapper>>();
            IDictionary<ulong, IDictionary<string, FrameFingerPrintWrapper>> videoFrameHashesToFileMap = MapVideoFrameHashesTo(metatable);
            foreach (PhotoFingerPrintWrapper wrapper in photoDatabase.PhotoFingerPrints)
            {
                // 1. Find bucket of possible candidates
                // 2. If there's only 1 result, assume it's the source video
                // 3. Otherwise, go through each possible video and run SSIM on it

            }

            return null;
        }

        private static IDictionary<ulong, IDictionary<string, FrameFingerPrintWrapper>> MapVideoFrameHashesTo(
            VideoFingerPrintDatabaseMetaTableWrapper metatable
        )
        {
            var hashToFileAndFrameMap = new Dictionary<ulong, IDictionary<string, FrameFingerPrintWrapper>>();
            foreach (VideoFingerPrintDatabaseMetaTableEntryWrapper wrapper in metatable.DatabaseMetaTableEntries)
            {
                VideoFingerPrintDatabaseWrapper database = VideoFingerPrintDatabaseLoader.Load(wrapper.FileName);
                foreach (VideoFingerPrintWrapper videoFingerPrint in database.VideoFingerPrints)
                {
                    foreach (FrameFingerPrintWrapper frameFingerPrint in videoFingerPrint.FingerPrints)
                    {
                        IDictionary<string, FrameFingerPrintWrapper> bucket;
                        if (hashToFileAndFrameMap.TryGetValue(frameFingerPrint.PHashCode, out bucket) == false)
                        {
                            bucket = new Dictionary<string, FrameFingerPrintWrapper>();
                            hashToFileAndFrameMap.Add(frameFingerPrint.PHashCode, bucket);
                        }

                        bucket.Add(videoFingerPrint.FilePath, frameFingerPrint);
                    }
                }
            }

            return hashToFileAndFrameMap;
        }
    }
}
