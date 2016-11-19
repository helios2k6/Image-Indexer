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

using FrameIndexLibrary;
using PhotoCollectionIndexer.Serialization;
using PhotoCollectionIndexer.Wrappers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoCollectionIndexer
{
    public static class Driver
    {
        enum Mode
        {
            INDEX,
            SEARCH,
            UNKNOWN,
        }

        #region public methods
        public static void Main(string[] args)
        {
            if (args.Length == 0 || IsHelp(args))
            {
                PrintHelp(null);
                return;
            }

            Mode mode = GetMode(args);

            if (mode == Mode.UNKNOWN)
            {
                PrintHelp("Unknown mode.");
                return;
            }

            if (mode == Mode.INDEX)
            {
                ExecuteIndex(args);
            }

            if (mode == Mode.SEARCH)
            {
                ExecuteSearch(args);
            }
        }
        #endregion

        #region private methods
        private static void ExecuteSearch(string[] args)
        {
            string photoFile = GetPhotoPath(args);
            string databaseFile = GetDatabasePath(args);

            if (string.IsNullOrWhiteSpace(photoFile) || string.IsNullOrWhiteSpace(databaseFile))
            {
                PrintHelp("Photo path or database path not provided");
                return;
            }

            if (File.Exists(photoFile) == false)
            {
                PrintHelp("Photo file does not exist");
                return;
            }

            if (File.Exists(databaseFile) == false)
            {
                PrintHelp("Database does not exist");
                return;
            }

            using (WritableLockBitImage lockbitImage = new WritableLockBitImage(Image.FromFile(photoFile), false))
            {

            }
        }

        private static void ExecuteIndex(string[] args)
        {
            string photoFile = GetPhotoPath(args);
            string databaseFile = GetDatabasePath(args);

            if (string.IsNullOrWhiteSpace(photoFile) || string.IsNullOrWhiteSpace(databaseFile))
            {
                PrintHelp("Video path or database path not provided");
                return;
            }

            if (File.Exists(photoFile) == false)
            {
                PrintHelp("Video file does not exist");
                return;
            }

            PhotoFingerPrintDatabaseWrapper database = File.Exists(databaseFile)
                ? DatabaseLoader.Load(databaseFile)
                : new PhotoFingerPrintDatabaseWrapper();

            using (WritableLockBitImage lockbitImage = new WritableLockBitImage(Image.FromFile(photoFile), false))
            {
                PhotoFingerPrintWrapper fingerPrint = new PhotoFingerPrintWrapper
                {
                    FilePath = photoFile,
                    PHash = FrameIndexer.IndexFrame(lockbitImage),
                };

                var fingerPrintList = new List<PhotoFingerPrintWrapper>();
                fingerPrintList.AddRange(database.PhotoFingerPrints);
                fingerPrintList.Add(fingerPrint);

                database.PhotoFingerPrints = fingerPrintList.ToArray();
                DatabaseSaver.Save(database, databaseFile);
            }
        }

        private static string GetDatabasePath(string[] args)
        {
            return GetArgumentTuple(args, "--database");
        }

        private static string GetPhotoPath(string[] args)
        {
            return GetArgumentTuple(args, "--photo");
        }

        private static string GetArgumentTuple(string[] args, string argSwitch)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string currentArg = args[i];
                if (string.Equals(currentArg, argSwitch))
                {
                    if (i + 1 >= args.Length)
                    {
                        return string.Empty;
                    }

                    return args[i + 1];
                }
            }

            return string.Empty;
        }

        private static Mode GetMode(string[] args)
        {
            foreach (var arg in args)
            {
                if (string.Equals(arg, "--index"))
                {
                    return Mode.INDEX;
                }

                if (string.Equals(arg, "--search"))
                {
                    return Mode.SEARCH;
                }
            }

            return Mode.UNKNOWN;
        }

        private static bool IsHelp(string[] args)
        {
            return args.Any(t => string.Equals(t, "--help", StringComparison.OrdinalIgnoreCase));
        }

        private static void PrintHelp(string messageToPrint)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Photo Indexer v1.0");

            if (string.IsNullOrWhiteSpace(messageToPrint) == false)
            {
                builder.Append("[INFO]: ").AppendLine(messageToPrint);
            }

            builder.AppendLine("Usage: <this program> [options]")
                .AppendLine()
                .AppendLine("Options:")
                .Append('\t').Append("--help").Append('\t').Append('\t').Append("Show this help text").AppendLine()
                .AppendLine()
                .AppendLine("Index Related Commands")
                .Append('\t').Append("--index").Append('\t').Append('\t').Append("Index a video").AppendLine()
                .Append('\t').Append("--photo").Append('\t').Append('\t').Append("The photo to index").AppendLine()
                .Append('\t').Append("--database").Append('\t').Append("The path to save the database to. This will update existing databases").AppendLine()
                .AppendLine()
                .AppendLine("Search Related Commands")
                .Append('\t').Append("--search").Append('\t').Append("Search for similar frames using an image").AppendLine()
                .Append('\t').Append("--photo").Append('\t').Append('\t').Append("The path to the photo you want to search for ").AppendLine()
                .Append('\t').Append("--database").Append('\t').Append("The path to the photo you want to use for ").AppendLine();

            Console.Write(builder.ToString());
        }
        #endregion



    }
}
