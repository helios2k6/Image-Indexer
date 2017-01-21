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
using System.IO;
using VideoIndexer.Wrappers;

namespace VideoIndexer.Serialization
{
    /// <summary>
    /// Saves a Database MetaTable to disk
    /// </summary>
    public static class DatabaseMetaTableSaver
    {
        #region private fields
        private static readonly int DefaultBufferSize = 4096;
        #endregion

        #region public methods
        /// <summary>
        /// Saves a Database MetaTable wrapper to disk
        /// </summary>
        /// <param name="databaseMetaTable"></param>
        /// <param name="filePath"></param>
        public static void Save(DatabaseMetaTableWrapper databaseMetaTable, string filePath)
        {
            byte[] rawDatabaseBytes = SaveDatabaseMetaTable(databaseMetaTable);
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                writer.Write(rawDatabaseBytes);
            }
        }

        /// <summary>
        /// Saves a Database MetaTable wrapper to disk
        /// </summary>
        /// <param name="databaseMetaTable"></param>
        /// <param name="outStream"></param>
        public static void Save(DatabaseMetaTableWrapper databaseMetaTable, Stream outStream)
        {
            byte[] buffer = SaveDatabaseMetaTable(databaseMetaTable);

            outStream.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region private methods
        private static byte[] SaveDatabaseMetaTable(DatabaseMetaTableWrapper wrapper)
        {
            var builder = new FlatBufferBuilder(DefaultBufferSize);
            CreateDatabaseMetaTable(wrapper, builder);

            return builder.SizedByteArray();
        }

        private static void CreateDatabaseMetaTable(DatabaseMetaTableWrapper wrapper, FlatBufferBuilder builder)
        {
            Offset<DatabaseMetaTableEntry>[] databaseMetaTableEntryArrayOffset = CreateDatabaseMetaTableEntryArray(wrapper, builder);

            VectorOffset entryArrayOffset = DatabaseMetaTable.CreateDatabaseMetaTableEntriesVector(builder, databaseMetaTableEntryArrayOffset);

            Offset<DatabaseMetaTable> databaseMetaTableOffset = DatabaseMetaTable.CreateDatabaseMetaTable(builder, entryArrayOffset);
            DatabaseMetaTable.FinishDatabaseMetaTableBuffer(builder, databaseMetaTableOffset);
        }

        private static Offset<DatabaseMetaTableEntry>[] CreateDatabaseMetaTableEntryArray(
            DatabaseMetaTableWrapper databaseMetaTableWrapper,
            FlatBufferBuilder builder
        )
        {
            int databaseMetaTableEntryCounter = 0;
            var databaseMetaTableEntryArrayOffset = new Offset<DatabaseMetaTableEntry>[databaseMetaTableWrapper.DatabaseMetaTableEntries.Length];
            foreach (DatabaseMetaTableEntryWrapper databaseMetaTableEntry in databaseMetaTableWrapper.DatabaseMetaTableEntries)
            {
                StringOffset fileNameOffset = builder.CreateString(databaseMetaTableEntry.FileName);

                DatabaseMetaTableEntry.StartDatabaseMetaTableEntry(builder);
                DatabaseMetaTableEntry.AddFileName(builder, fileNameOffset);
                DatabaseMetaTableEntry.AddFileSize(builder, databaseMetaTableEntry.FileSize);

                databaseMetaTableEntryArrayOffset[databaseMetaTableEntryCounter] = DatabaseMetaTableEntry.EndDatabaseMetaTableEntry(builder);
                databaseMetaTableEntryCounter++;
            }

            return databaseMetaTableEntryArrayOffset;
        }
        #endregion
    }
}
