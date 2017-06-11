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

using Core.Model.Wrappers;
using Core.Utils;
using FlatBuffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.Model.Serialization
{
    /// <summary>
    /// Loads the database from a file
    /// </summary>
    public static class VideoFingerPrintDatabaseLoader
    {
        private static readonly int DefaultBufferSize = 1024;

        #region public methods
        /// <summary>
        /// Load a FlatBuffer database of video fingerprints
        /// </summary>
        public static VideoFingerPrintDatabaseWrapper Load(string path)
        {
            return Convert(LoadDatabase(path));
        }

        /// <summary>
        /// Load a FlatBuffer database of video fingerprints using raw bytes
        /// </summary>
        /// <param name="rawBytes">The raws bytes of the database</param>
        /// <returns>A loaded database</returns>
        public static VideoFingerPrintDatabaseWrapper Load(byte[] rawBytes)
        {
            return Convert(VideoFingerPrintDatabase.GetRootAsVideoFingerPrintDatabase(new ByteBuffer(rawBytes)));
        }
        #endregion

        #region private methods
        private static VideoFingerPrintDatabase LoadDatabase(string path)
        {
            if (File.Exists(path) == false)
            {
                throw new ArgumentException();
            }

            using (var memoryStream = new MemoryStream())
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                var buffer = new byte[DefaultBufferSize];
                int count = 0;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    memoryStream.Write(buffer, 0, count);
                }

                return VideoFingerPrintDatabase.GetRootAsVideoFingerPrintDatabase(new ByteBuffer(memoryStream.ToArray()));
            }
        }

        private static VideoFingerPrintDatabaseWrapper Convert(VideoFingerPrintDatabase database)
        {
            IEnumerable<VideoFingerPrintWrapper> videoFingerPrints = from i in Enumerable.Range(0, database.VideoFingerPrintsLength)
                                                                     select Convert(database.VideoFingerPrints(i));
            return new VideoFingerPrintDatabaseWrapper
            {
                VideoFingerPrints = videoFingerPrints.ToArray(),
            };
        }

        private static VideoFingerPrintWrapper Convert(VideoFingerPrint? videoFingerPrint)
        {
            VideoFingerPrint videoFingerPrintNotNull = TypeUtils.NullThrows(videoFingerPrint);
            IEnumerable<FrameFingerPrintWrapper> frameFingerPrints = from i in Enumerable.Range(0, videoFingerPrintNotNull.FrameFingerPrintsLength)
                                                                     select Convert(videoFingerPrintNotNull.FrameFingerPrints(i));
            return new VideoFingerPrintWrapper
            {
                FilePath = videoFingerPrintNotNull.FilePath,
                FingerPrints = frameFingerPrints.ToArray(),
            };
        }

        private static FrameFingerPrintWrapper Convert(FrameFingerPrint? frameFingerPrint)
        {
            FrameFingerPrint frameFingerPrintNotNull = TypeUtils.NullThrows(frameFingerPrint);
            byte[] edgeGrayScaleThumb = (from i in Enumerable.Range(0, frameFingerPrintNotNull.EdgeGrayScaleThumbLength)
                                         select frameFingerPrintNotNull.EdgeGrayScaleThumb(i)).ToArray();
            return new FrameFingerPrintWrapper
            {
                FrameNumber = frameFingerPrintNotNull.FrameNumber,
                PHashCode = frameFingerPrintNotNull.PHash,
                EdgeGrayScaleThumb = edgeGrayScaleThumb,
            };
        }
        #endregion
    }
}
