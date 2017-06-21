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


using Core;
using Core.Compression;
using Core.Model.Wrappers;
using FlatBuffers;
using System.IO;

namespace Core.Model.Serialization
{
    public static class PhotoFingerPrintDatabaseSaver
    {
        #region private fields
        private static readonly int DefaultBufferSize = 4096;
        #endregion

        #region public methods
        /// <summary>
        /// Save a fingerprint database to a file
        /// </summary>
        /// <param name="database">The database to save</param>
        /// <param name="filePath">The file path to save it to</param>
        public static void Save(PhotoFingerPrintDatabaseWrapper database, string filePath)
        {
            byte[] rawDatabaseBytes = SaveDatabase(database);
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                writer.Write(rawDatabaseBytes);
            }
        }

        /// <summary>
        /// Save a fingerprint database to a stream
        /// </summary>
        /// <param name="database">The database to save</param>
        /// <param name="outStream">The stream to write to</param>
        public static void Save(PhotoFingerPrintDatabaseWrapper database, Stream outStream)
        {
            byte[] buffer = SaveDatabase(database);

            outStream.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region private methods
        private static byte[] SaveDatabase(PhotoFingerPrintDatabaseWrapper database)
        {
            var builder = new FlatBufferBuilder(DefaultBufferSize);
            CreatePhotoFingerPrintDatabase(database, builder);

            return builder.SizedByteArray();
        }

        private static void CreatePhotoFingerPrintDatabase(PhotoFingerPrintDatabaseWrapper database, FlatBufferBuilder builder)
        {
            Offset<PhotoFingerPrint>[] photoFingerPrintArray = CreatePhotoFingerPrintArray(database, builder);

            VectorOffset photoFingerPrintDatabaseVectorOffset = PhotoFingerPrintDatabase.CreateFingerPrintsVector(builder, photoFingerPrintArray);
            Offset<PhotoFingerPrintDatabase> databaseOffset = PhotoFingerPrintDatabase.CreatePhotoFingerPrintDatabase(builder, photoFingerPrintDatabaseVectorOffset);
            PhotoFingerPrintDatabase.FinishPhotoFingerPrintDatabaseBuffer(builder, databaseOffset);
        }

        private static Offset<PhotoFingerPrint>[] CreatePhotoFingerPrintArray(PhotoFingerPrintDatabaseWrapper database, FlatBufferBuilder builder)
        {
            int photoFingerPrintCounter = 0;
            var photoFingerPrintArray = new Offset<PhotoFingerPrint>[database.PhotoFingerPrints.Length];
            foreach (PhotoFingerPrintWrapper fingerPrint in database.PhotoFingerPrints)
            {
                StringOffset filePathOffset = builder.CreateString(fingerPrint.FilePath);
                VectorOffset grayScaleImageOffset = PhotoFingerPrint.CreateEdgeGrayScaleThumbVector(builder, SerializationUtils.CompressedGrayScaleThumb(fingerPrint.EdgeGrayScaleThumb));

                PhotoFingerPrint.StartPhotoFingerPrint(builder);
                PhotoFingerPrint.AddFilePath(builder, filePathOffset);
                PhotoFingerPrint.AddPhash(builder, fingerPrint.PHash);
                PhotoFingerPrint.AddEdgeGrayScaleThumb(builder, grayScaleImageOffset);

                photoFingerPrintArray[photoFingerPrintCounter] = PhotoFingerPrint.EndPhotoFingerPrint(builder);
                photoFingerPrintCounter++;
            }

            return photoFingerPrintArray;
        }
        #endregion
    }
}
