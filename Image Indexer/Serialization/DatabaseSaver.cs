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
using System.Drawing;
using System.IO;

namespace ImageIndexer
{
    /// <summary>
    /// Saves the database
    /// </summary>
    public static class DatabaseSaver
    {
        #region private fields
        private static readonly int DefaultBufferSize = 1024;
        #endregion

        #region public methods
        /// <summary>
        /// Save a fingerprint database to a file
        /// </summary>
        /// <param name="database">The database to save</param>
        /// <param name="filePath">The file path to save it to</param>
        public static void Save(VideoFingerPrintDatabaseWrapper database, string filePath)
        {
            byte[] rawDatabaseBytes = SaveDatabase(database);
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.OpenOrCreate)))
            {
                writer.Write(rawDatabaseBytes);
            }
        }

        /// <summary>
        /// Save a fingerprint database to a stream
        /// </summary>
        /// <param name="database">The database to save</param>
        /// <param name="outStream">The stream to write to</param>
        public static void Save(VideoFingerPrintDatabaseWrapper database, Stream outStream)
        {
            byte[] buffer = SaveDatabase(database);

            outStream.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region private methods
        private static byte[] SaveDatabase(VideoFingerPrintDatabaseWrapper database)
        {
            var builder = new FlatBufferBuilder(DefaultBufferSize);
            CreateVideoFingerPrintDatabase(database, builder);

            return builder.SizedByteArray();
        }

        private static void CreateVideoFingerPrintDatabase(VideoFingerPrintDatabaseWrapper database, FlatBufferBuilder builder)
        {
            Offset<VideoFingerPrint>[] videoFingerPrintArray = CreateVideoFingerPrintArray(database, builder);

            VectorOffset videoFingerPrintDatabaseVectorOffset = VideoFingerPrintDatabase.CreateVideoFingerPrintsVector(builder, videoFingerPrintArray);

            Offset<VideoFingerPrintDatabase> databaseOffset = VideoFingerPrintDatabase.CreateVideoFingerPrintDatabase(builder, videoFingerPrintDatabaseVectorOffset);
            VideoFingerPrintDatabase.FinishVideoFingerPrintDatabaseBuffer(builder, databaseOffset);
        }

        private static Offset<VideoFingerPrint>[] CreateVideoFingerPrintArray(VideoFingerPrintDatabaseWrapper database, FlatBufferBuilder builder)
        {
            int videoFingerPrintCounter = 0;
            var videoFingerPrintArray = new Offset<VideoFingerPrint>[database.VideoFingerPrints.Length];
            foreach (VideoFingerPrintWrapper videoFingerPrint in database.VideoFingerPrints)
            {
                Offset<FrameFingerPrint>[] frameFingerPrintArray = CreateFrameFingerPrintArray(builder, videoFingerPrint);

                StringOffset videoFilePath = builder.CreateString(videoFingerPrint.FilePath);
                VectorOffset frameFingerPrintVectorOffset = VideoFingerPrint.CreateFrameFingerPrintsVector(builder, frameFingerPrintArray);

                VideoFingerPrint.StartVideoFingerPrint(builder);
                VideoFingerPrint.AddFilePath(builder, videoFilePath);
                VideoFingerPrint.AddFrameFingerPrints(builder, frameFingerPrintVectorOffset);

                videoFingerPrintArray[videoFingerPrintCounter] = VideoFingerPrint.EndVideoFingerPrint(builder);
                videoFingerPrintCounter++;
            }

            return videoFingerPrintArray;
        }

        private static Offset<FrameFingerPrint>[] CreateFrameFingerPrintArray(FlatBufferBuilder builder, VideoFingerPrintWrapper videoFingerPrint)
        {
            int frameFingerPrintCounter = 0;
            var frameFingerPrintArray = new Offset<FrameFingerPrint>[videoFingerPrint.FingerPrints.Length];
            foreach (FrameFingerPrintWrapper frameFingerPrint in videoFingerPrint.FingerPrints)
            {
                Offset<Macroblock>[] macroblockArray = CreateMacroblockArray(builder, frameFingerPrint);

                VectorOffset macroblockVectorOffset = FrameFingerPrint.CreateMacroblocksVector(builder, macroblockArray);

                FrameFingerPrint.StartFrameFingerPrint(builder);
                FrameFingerPrint.AddFrameNumber(builder, frameFingerPrint.FrameNumber);
                FrameFingerPrint.AddMacroblocks(builder, macroblockVectorOffset);

                frameFingerPrintArray[frameFingerPrintCounter] = FrameFingerPrint.EndFrameFingerPrint(builder);
                frameFingerPrintCounter++;
            }

            return frameFingerPrintArray;
        }

        private static Offset<Macroblock>[] CreateMacroblockArray(FlatBufferBuilder builder, FrameFingerPrintWrapper frameFingerPrint)
        {
            int macroblockCounter = 0;
            var macroblockArray = new Offset<Macroblock>[frameFingerPrint.Macroblocks.Length];
            foreach (MacroblockWrapper macroblock in frameFingerPrint.Macroblocks)
            {
                Macroblock.StartPixelsVector(builder, macroblock.Pixels.Length);
                foreach (Color pixel in macroblock.Pixels)
                {
                    Pixel.CreatePixel(builder, pixel.R, pixel.G, pixel.B);
                }
                VectorOffset macroblockPixelOffset = builder.EndVector();

                macroblockArray[macroblockCounter] = Macroblock.CreateMacroblock(builder, macroblock.Width, macroblock.Height, macroblockPixelOffset);
                macroblockCounter++;
            }

            return macroblockArray;
        }
        #endregion
    }
}