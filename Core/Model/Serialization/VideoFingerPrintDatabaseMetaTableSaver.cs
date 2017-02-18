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
using FlatBuffers;
using System.IO;

namespace Core.Model.Serialization
{
    /// <summary>
    /// Saves a Database MetaTable to disk
    /// </summary>
    public static class VideoFingerPrintDatabaseMetaTableSaver
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
        public static void Save(VideoFingerPrintDatabaseMetaTableWrapper databaseMetaTable, string filePath)
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
        public static void Save(VideoFingerPrintDatabaseMetaTableWrapper databaseMetaTable, Stream outStream)
        {
            byte[] buffer = SaveDatabaseMetaTable(databaseMetaTable);

            outStream.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region private methods
        private static byte[] SaveDatabaseMetaTable(VideoFingerPrintDatabaseMetaTableWrapper wrapper)
        {
            var builder = new FlatBufferBuilder(DefaultBufferSize);
            CreateDatabaseMetaTable(wrapper, builder);

            return builder.SizedByteArray();
        }

        private static void CreateDatabaseMetaTable(VideoFingerPrintDatabaseMetaTableWrapper wrapper, FlatBufferBuilder builder)
        {
            Offset<VideoFingerPrintDatabaseMetaTableEntry>[] databaseMetaTableEntryArrayOffset = CreateDatabaseMetaTableEntryArray(wrapper, builder);

            VectorOffset entryArrayOffset = VideoFingerPrintDatabaseMetaTable.CreateDatabaseMetaTableEntriesVector(builder, databaseMetaTableEntryArrayOffset);

            Offset<VideoFingerPrintDatabaseMetaTable> databaseMetaTableOffset = VideoFingerPrintDatabaseMetaTable.CreateVideoFingerPrintDatabaseMetaTable(builder, entryArrayOffset);
            VideoFingerPrintDatabaseMetaTable.FinishVideoFingerPrintDatabaseMetaTableBuffer(builder, databaseMetaTableOffset);
        }

        private static Offset<VideoFingerPrintDatabaseMetaTableEntry>[] CreateDatabaseMetaTableEntryArray(
            VideoFingerPrintDatabaseMetaTableWrapper databaseMetaTableWrapper,
            FlatBufferBuilder builder
        )
        {
            int databaseMetaTableEntryCounter = 0;
            var databaseMetaTableEntryArrayOffset = new Offset<VideoFingerPrintDatabaseMetaTableEntry>[databaseMetaTableWrapper.DatabaseMetaTableEntries.Length];
            foreach (VideoFingerPrintDatabaseMetaTableEntryWrapper databaseMetaTableEntry in databaseMetaTableWrapper.DatabaseMetaTableEntries)
            {
                StringOffset fileNameOffset = builder.CreateString(databaseMetaTableEntry.FileName);

                VideoFingerPrintDatabaseMetaTableEntry.StartVideoFingerPrintDatabaseMetaTableEntry(builder);
                VideoFingerPrintDatabaseMetaTableEntry.AddFileName(builder, fileNameOffset);
                VideoFingerPrintDatabaseMetaTableEntry.AddFileSize(builder, databaseMetaTableEntry.FileSize);

                databaseMetaTableEntryArrayOffset[databaseMetaTableEntryCounter] = VideoFingerPrintDatabaseMetaTableEntry.EndVideoFingerPrintDatabaseMetaTableEntry(builder);
                databaseMetaTableEntryCounter++;
            }

            return databaseMetaTableEntryArrayOffset;
        }
        #endregion
    }
}
