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

using FlatBuffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VideoIndexer.Wrappers;

namespace VideoIndexer.Serialization
{
    /// <summary>
    /// Loads a database metatable from disk
    /// </summary>
    public static class DatabaseMetaTableLoader
    {
        private static readonly int DefaultBufferSize = 512;

        #region public method
        /// <summary>
        /// Load a database metatable from disk
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>A database</returns>
        public static DatabaseMetaTableWrapper Load(string path)
        {
            return Convert(LoadMetaTable(path));
        }

        /// <summary>
        /// Load a database metatable from raw bytes
        /// </summary>
        /// <param name="rawBytes"></param>
        /// <returns></returns>
        public static DatabaseMetaTableWrapper Load(byte[] rawBytes)
        {
            return Convert(DatabaseMetaTable.GetRootAsDatabaseMetaTable(new ByteBuffer(rawBytes)));
        }
        #endregion

        #region private methods
        private static DatabaseMetaTable LoadMetaTable(string path)
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

                return DatabaseMetaTable.GetRootAsDatabaseMetaTable(new ByteBuffer(memoryStream.ToArray()));
            }
        }

        private static DatabaseMetaTableWrapper Convert(DatabaseMetaTable databaseMetaTable)
        {
            IEnumerable<DatabaseMetaTableEntryWrapper> metaTableEntries = from i in Enumerable.Range(0, databaseMetaTable.DatabaseMetaTableEntriesLength)
                                                                          select Convert(databaseMetaTable.GetDatabaseMetaTableEntries(i));
            return new DatabaseMetaTableWrapper
            {
                DatabaseMetaTableEntries = metaTableEntries.ToArray(),
            };
        }

        private static DatabaseMetaTableEntryWrapper Convert(DatabaseMetaTableEntry entry)
        {
            return new DatabaseMetaTableEntryWrapper
            {
                FileName = entry.FileName,
                FileSize = entry.FileSize,
            };
        }
        #endregion
    }
}
