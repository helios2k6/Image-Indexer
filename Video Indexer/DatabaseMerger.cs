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
using System.Linq;
using VideoIndexer.Wrappers;

namespace VideoIndexer
{
    public static class DatabaseMerger
    {
        #region public methods
        public static VideoFingerPrintDatabaseWrapper Merge(
            VideoFingerPrintDatabaseWrapper first,
            VideoFingerPrintDatabaseWrapper second
        )
        {
            var videoFingerPrintsByFile = new Dictionary<string, VideoFingerPrintWrapper>();
            foreach (VideoFingerPrintWrapper fingerPrint in first.VideoFingerPrints)
            {
                videoFingerPrintsByFile.Add(fingerPrint.FilePath, fingerPrint);
            }

            foreach (VideoFingerPrintWrapper fingerPrint in second.VideoFingerPrints)
            {
                if (videoFingerPrintsByFile.ContainsKey(fingerPrint.FilePath))
                {
                    // Handle collision

                }
                else
                {
                    // Just add it if there's no collision
                    videoFingerPrintsByFile.Add(fingerPrint.FilePath, fingerPrint);
                }
            }

            return null;
        }
        #endregion
        #region private methods
        #endregion
    }
}