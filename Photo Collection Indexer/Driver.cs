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

using Core.DSA;
using Core.Model.Serialization;
using Core.Model.Wrappers;
using FrameIndexLibrary;
using PhotoCollectionIndexer.Executors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            using (Image frame = Image.FromFile(photoFile))
            {
                PhotoFingerPrintDatabaseWrapper database = PhotoFingerPrintDatabaseLoader.Load(databaseFile);
                ulong imageHash = FrameIndexer.CalculateFramePerceptionHashOnly(frame);

                var results = from fingerPrint in database.PhotoFingerPrints.AsParallel()
                              let distance = DistanceCalculator.CalculateHammingDistance(imageHash, fingerPrint.PHash)
                              where distance < 5
                              orderby distance
                              select new
                              {
                                  Distance = distance,
                                  FilePath = fingerPrint.FilePath
                              };

                foreach (var result in results)
                {
                    Console.WriteLine(string.Format("{0} - {1}", result.Distance, result.FilePath));
                }
            }
        }

        private static void ExecuteIndex(string[] args)
        {
            IEnumerable<string> photoFiles = GetPhotoPaths(args).Distinct();
            string databaseFile = GetDatabasePath(args);

            if (photoFiles.Any() == false || string.IsNullOrWhiteSpace(databaseFile))
            {
                PrintHelp("Photo file or database path not provided");
                return;
            }

            PhotoFingerPrintDatabaseWrapper database = File.Exists(databaseFile)
                ? PhotoFingerPrintDatabaseLoader.Load(databaseFile)
                : new PhotoFingerPrintDatabaseWrapper();

            IEnumerable<PhotoFingerPrintWrapper> fingerPrintBag = IndexPhotosImpl(photoFiles, database);

            database.PhotoFingerPrints = fingerPrintBag.ToArray();
            PhotoFingerPrintDatabaseSaver.Save(database, databaseFile);
        }

        private static IEnumerable<PhotoFingerPrintWrapper> IndexPhotosImpl(IEnumerable<string> photoFiles, PhotoFingerPrintDatabaseWrapper database)
        {
            PhotoFileReaderExecutor photoReader = new PhotoFileReaderExecutor(photoFiles, 3);
            PhotoFileIndexerExecutor photoIndexer = new PhotoFileIndexerExecutor(photoReader, 4);

            Task photoReaderTask = photoReader.LoadPhotos();
            Task photoIndexerTask = photoIndexer.Start();
            Task.WaitAll(photoReaderTask, photoIndexerTask);

            return photoIndexer.GetFingerPrints();
        }

        private static string GetDatabasePath(string[] args)
        {
            return GetArgumentTuple(args, "--database");
        }

        private static string GetPhotoPath(string[] args)
        {
            return GetArgumentTuple(args, "--photo");
        }

        private static bool IsGlobbedPath(string path)
        {
            return path.IndexOf("*") != -1;
        }

        private static IEnumerable<string> GetPhotoPaths(string[] args)
        {
            bool collecting = false;
            bool isRecursive = IsRecursive(args);
            for (int i = 0; i < args.Length; i++)
            {
                string currentArg = args[i];
                if (string.Equals(currentArg, "--photos"))
                {
                    collecting = true;
                    continue;
                }

                if (collecting && currentArg.IndexOf("--") == -1)
                {
                    if (IsGlobbedPath(currentArg))
                    {
                        foreach (string globbedPhotoPath in FileUtils.Glob(currentArg).Where(IsPhotoFile))
                        {
                            yield return globbedPhotoPath;
                        }
                    }
                    else if (Directory.Exists(currentArg))
                    {
                        foreach (string photo in ProcessDirectory(currentArg, isRecursive))
                        {
                            yield return photo;
                        }
                    }
                    else
                    {
                        if (IsPhotoFile(currentArg) && File.Exists(currentArg))
                        {
                            yield return currentArg;
                        }
                    }
                }
                else if (collecting && currentArg.IndexOf("--") != -1)
                {
                    yield break;
                }
            }
        }

        private static IEnumerable<string> ProcessDirectory(string directory, bool isRecursive)
        {
            if (isRecursive == false)
            {
                foreach (string photo in Directory.EnumerateFiles(directory).Where(IsPhotoFile))
                {
                    yield return photo;
                }
            }
            else
            {
                foreach (string subdirectory in Directory.EnumerateDirectories(directory))
                {
                    foreach (string file in ProcessDirectory(subdirectory, isRecursive))
                    {
                        yield return file;
                    }
                }

                foreach (string photo in Directory.EnumerateFiles(directory).Where(IsPhotoFile))
                {
                    yield return photo;
                }
            }
        }

        private static bool IsPhotoFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return string.Equals(".png", extension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".jpg", extension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".jpeg", extension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".gif", extension, StringComparison.OrdinalIgnoreCase);
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

        private static bool IsRecursive(string[] args)
        {
            return args.Any(t => string.Equals(t, "--recursive", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsHelp(string[] args)
        {
            return args.Any(t => string.Equals(t, "--help", StringComparison.OrdinalIgnoreCase));
        }

        private static void PrintHelp(string messageToPrint)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Photo Indexer v2.0");

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
                .Append('\t').Append("--index").Append('\t').Append('\t').Append("Index a photo or a set of photos").AppendLine()
                .Append('\t').Append("--photos").Append('\t').Append("The photos to index. Can use globbing for this or --recursive to search sub-directories").AppendLine()
                .Append('\t').Append("--recursive").Append('\t').Append("If the supplied --photos argument is a directory, this switch allows you to search recursively for photo files").AppendLine()
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
