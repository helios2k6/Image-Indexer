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
using Core.Utils;
using FlatBuffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.Model.Serialization
{
    public static class PhotoFingerPrintDatabaseLoader
    {
        #region private fields
        private static readonly int DefaultBufferSize = 1024;
        #endregion

        #region public methods
        /// <summary>
        /// Load a FlatBuffer database of video fingerprints
        /// </summary>
        public static PhotoFingerPrintDatabaseWrapper Load(string path)
        {
            return Convert(LoadDatabase(path));
        }

        /// <summary>
        /// Load a FlatBuffer database of video fingerprints using raw bytes
        /// </summary>
        /// <param name="rawBytes">The raws bytes of the database</param>
        /// <returns>A loaded database</returns>
        public static PhotoFingerPrintDatabaseWrapper Load(byte[] rawBytes)
        {
            return Convert(PhotoFingerPrintDatabase.GetRootAsPhotoFingerPrintDatabase(new ByteBuffer(rawBytes)));
        }
        #endregion

        #region private methods
        private static PhotoFingerPrintDatabase LoadDatabase(string path)
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

                return PhotoFingerPrintDatabase.GetRootAsPhotoFingerPrintDatabase(new ByteBuffer(memoryStream.ToArray()));
            }
        }

        private static PhotoFingerPrintDatabaseWrapper Convert(PhotoFingerPrintDatabase database)
        {
            IEnumerable<PhotoFingerPrintWrapper> fingerPrints = from i in Enumerable.Range(0, database.FingerPrintsLength)
                                                                select Convert(database.FingerPrints(i));

            return new PhotoFingerPrintDatabaseWrapper
            {
                PhotoFingerPrints = fingerPrints.ToArray(),
            };
        }

        private static PhotoFingerPrintWrapper Convert(PhotoFingerPrint? fingerPrint)
        {
            PhotoFingerPrint fingerPrintNotNull = TypeUtils.NullThrows(fingerPrint);
            byte[] edgeGrayScaleThumb = (
                from i in Enumerable.Range(0, fingerPrintNotNull.EdgeGrayScaleThumbLength)
                select fingerPrintNotNull.EdgeGrayScaleThumb(i)
            ).ToArray();

            return new PhotoFingerPrintWrapper
            {
                FilePath = fingerPrintNotNull.FilePath,
                PHash = fingerPrintNotNull.Phash,
                EdgeGrayScaleThumb = edgeGrayScaleThumb,
            };
        }
        #endregion
    }
}
