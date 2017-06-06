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
    /// Loads a database metatable from disk
    /// </summary>
    public static class VideoFingerPrintDatabaseMetaTableLoader
    {
        private static readonly int DefaultBufferSize = 512;

        #region public method
        /// <summary>
        /// Load a database metatable from disk
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>A database</returns>
        public static VideoFingerPrintDatabaseMetaTableWrapper Load(string path)
        {
            return Convert(LoadMetaTable(path));
        }

        /// <summary>
        /// Load a database metatable from raw bytes
        /// </summary>
        /// <param name="rawBytes"></param>
        /// <returns></returns>
        public static VideoFingerPrintDatabaseMetaTableWrapper Load(byte[] rawBytes)
        {
            return Convert(VideoFingerPrintDatabaseMetaTable.GetRootAsVideoFingerPrintDatabaseMetaTable(new ByteBuffer(rawBytes)));
        }
        #endregion

        #region private methods
        private static VideoFingerPrintDatabaseMetaTable LoadMetaTable(string path)
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

                return VideoFingerPrintDatabaseMetaTable.GetRootAsVideoFingerPrintDatabaseMetaTable(new ByteBuffer(memoryStream.ToArray()));
            }
        }

        private static VideoFingerPrintDatabaseMetaTableWrapper Convert(VideoFingerPrintDatabaseMetaTable databaseMetaTable)
        {
            IEnumerable<VideoFingerPrintDatabaseMetaTableEntryWrapper> metaTableEntries = from i in Enumerable.Range(0, databaseMetaTable.DatabaseMetaTableEntriesLength)
                                                                                          select Convert(databaseMetaTable.DatabaseMetaTableEntries(i));
            return new VideoFingerPrintDatabaseMetaTableWrapper
            {
                DatabaseMetaTableEntries = metaTableEntries.ToArray(),
            };
        }

        private static VideoFingerPrintDatabaseMetaTableEntryWrapper Convert(VideoFingerPrintDatabaseMetaTableEntry? entry)
        {
            VideoFingerPrintDatabaseMetaTableEntry entryNotNull = TypeUtils.NullThrows(entry);
            return new VideoFingerPrintDatabaseMetaTableEntryWrapper
            {
                FileName = entryNotNull.FileName,
                FileSize = entryNotNull.FileSize,
            };
        }
        #endregion
    }
}
