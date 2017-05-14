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
using Core.Metrics;
using Core.Model.Serialization;
using Core.Model.Utils;
using Core.Model.Wrappers;
using Core.Modes;
using FrameIndexLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoIndex.Video;
using VideoIndexer.Merger;

namespace VideoIndexer.Driver
{
    internal static class Driver
    {
        #region enums
        private enum Mode
        {
            INDEX,
            SEARCH,
            CHECK,
            COALESCE,
            UNKNOWN,
        }
        #endregion
        #region private fields
        private const long DefaultMaxMemory = 4294967296; // 4 Gibibytes
        #endregion
        #region driver
        public static void Main(string[] args)
        {
            if (args.Length == 0 || IsHelp(args))
            {
                PrintHelp(null);
                return;
            }

            switch (GetMode(args))
            {
                case Mode.CHECK:
                    ExecuteCheckDatabase(args);
                    break;
                case Mode.COALESCE:
                    ExecuteCoalesceMetatable(args);
                    break;
                case Mode.INDEX:
                    ExecuteIndex(args);
                    break;
                case Mode.SEARCH:
                    ExecuteSearch(args);
                    break;
                case Mode.UNKNOWN:
                    PrintHelp("Unknown mode.");
                    break;
            }
        }
        #endregion
        #region private methods
        #region index
        private static void ExecuteIndex(string[] args)
        {
            string videoFile = GetVideoPath(args);
            string metatable = GetDatabaseMetaTable(args);
            string numThreadsArg = GetNumThreads(args);
            string maxMemoryArg = GetMaxMemory(args);

            if (string.IsNullOrWhiteSpace(videoFile) || string.IsNullOrWhiteSpace(metatable))
            {
                PrintHelp("Video path or database path not provided");
                return;
            }

            int numThreads = 1;
            if (string.IsNullOrWhiteSpace(numThreadsArg) == false)
            {
                int.TryParse(numThreadsArg, out numThreads);
            }

            long maxMemory = 0;
            if (long.TryParse(maxMemoryArg, out maxMemory) == false)
            {
                maxMemory = DefaultMaxMemory;
            }

            if (maxMemory <= 0)
            {
                PrintHelp("--max-memory must be greater than 0");
                return;
            }

            IndexImpl(metatable, GetVideoFiles(videoFile), numThreads, maxMemory);
        }

        private static void IndexImpl(string databaseMetaTablePath, IEnumerable<string> videoPaths, int numThreads, long maxMemory)
        {
            IEnumerable<string> knownHashes = GetKnownDatabaseEntries(databaseMetaTablePath);
            var skippedFiles = new ConcurrentBag<string>();

            using (ChangeErrorMode _ = new ChangeErrorMode(ChangeErrorMode.ErrorModes.FailCriticalErrors | ChangeErrorMode.ErrorModes.NoGpFaultErrorBox))
            using (FingerPrintStore store = new FingerPrintStore(databaseMetaTablePath))
            {
                long maxMemoryPerIndexJob = (long)Math.Round((double)maxMemory / numThreads);
                Parallel.ForEach(
                    videoPaths.Except(knownHashes),
                    new ParallelOptions { MaxDegreeOfParallelism = numThreads },
                    videoPath =>
                    {
                        string fileName = Path.GetFileName(videoPath);
                        try
                        {
                            if (videoPath.Length > 260) // Max file path that mediainfo can handle
                            {
                                Console.WriteLine("File path is too long. Skipping: " + fileName);
                                return;
                            }

                            VideoFingerPrintWrapper videoFingerPrint = Video.VideoIndexer.IndexVideo(videoPath, maxMemoryPerIndexJob);
                            store.AddFingerPrint(videoFingerPrint);
                        }
                        catch (InvalidOperationException e)
                        {
                            Console.WriteLine(string.Format("Unable to hash file {0}. Reason: {1}. Skipping", fileName, e.Message));
                            skippedFiles.Add(videoPath);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(string.Format("Unable to hash file {0}. Reason: {1}. Skipping", fileName, e.Message));
                        }
                    }
                );

                // Attempting to index skipped files
                foreach (string skippedFile in skippedFiles)
                {
                    Console.WriteLine("Attempting to hash skipped file: " + skippedFile);
                    try
                    {
                        VideoFingerPrintWrapper videoFingerPrint = Video.VideoIndexer.IndexVideo(skippedFile, maxMemory);
                        store.AddFingerPrint(videoFingerPrint);
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine(string.Format("Unable to hash file {0} for the second time. Giving up", skippedFile));
                    }
                }

                store.Shutdown();
                store.Wait();
            }
        }

        private static IEnumerable<string> GetKnownDatabaseEntries(string databaseMetaTablePath)
        {
            if (File.Exists(databaseMetaTablePath) == false)
            {
                return Enumerable.Empty<string>();
            }

            var knownDatabaseEntries = new HashSet<string>();
            VideoFingerPrintDatabaseMetaTableWrapper metatable = VideoFingerPrintDatabaseMetaTableLoader.Load(databaseMetaTablePath);
            foreach (VideoFingerPrintDatabaseMetaTableEntryWrapper entry in metatable.DatabaseMetaTableEntries)
            {
                VideoFingerPrintDatabaseWrapper database = VideoFingerPrintDatabaseLoader.Load(entry.FileName);
                foreach (VideoFingerPrintWrapper fingerprint in database.VideoFingerPrints)
                {
                    knownDatabaseEntries.Add(fingerprint.FilePath);
                }
            }

            return knownDatabaseEntries;
        }

        private static IEnumerable<string> GetVideoFiles(string path)
        {
            if (File.Exists(path))
            {
                return new[] { path };
            }

            if (Directory.Exists(path) == false)
            {
                return Enumerable.Empty<string>();
            }

            return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Where(IsVideoFile);
        }

        private static bool IsVideoFile(string path)
        {
            string extension = Path.GetExtension(path);
            return string.Equals(".mkv", extension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".mp4", extension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".avi", extension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".ogm", extension, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(".wmv", extension, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
        #region search
        private static void ExecuteSearch(string[] args)
        {
            string photoFilePath = GetPhotoPath(args);
            string databaseMetaTablePath = GetDatabaseMetaTable(args);

            if (string.IsNullOrWhiteSpace(photoFilePath) || string.IsNullOrWhiteSpace(databaseMetaTablePath))
            {
                PrintHelp("Photo path or database metatable path not provided");
                return;
            }

            if (File.Exists(photoFilePath) == false)
            {
                PrintHelp("Photo file does not exist");
                return;
            }

            if (File.Exists(databaseMetaTablePath) == false)
            {
                PrintHelp("Database MetaTable does not exist");
                return;
            }

            using (Image frame = Image.FromFile(photoFilePath))
            {
                ulong providedPhotoHash = FrameIndexer.IndexFrame(frame);
                VideoFingerPrintDatabaseMetaTableWrapper metaTable = VideoFingerPrintDatabaseMetaTableLoader.Load(databaseMetaTablePath);
                BKTree<FrameMetricWrapper> bktree = ModelMetricUtils.CreateBKTree(metaTable);
                IDictionary<FrameMetricWrapper, int> treeResults = bktree.Query(
                    new PhotoMetricWrapper
                    {
                        Photo = new PhotoFingerPrintWrapper
                        {
                            FilePath = photoFilePath,
                            PHash = providedPhotoHash,
                        },
                    },
                    2
                );

                foreach (KeyValuePair<FrameMetricWrapper, int> kvp in treeResults.OrderBy(e => e.Value))
                {
                    FrameMetricWrapper frameWrapper = kvp.Key;
                    int distance = kvp.Value;
                    Console.WriteLine(string.Format("Distance {0} for {1} at Frame {2}", distance, frameWrapper.Video.FilePath, frameWrapper.Frame.FrameNumber));
                }
            }
        }
        #endregion
        #region check database
        private static void ExecuteCheckDatabase(string[] args)
        {
            string maxMemoryArg = GetMaxMemory(args);
            string databaseMetaTablePath = GetDatabaseMetaTable(args);

            if (string.IsNullOrWhiteSpace(databaseMetaTablePath))
            {
                PrintHelp("Database metatable path not provided");
                return;
            }

            if (File.Exists(databaseMetaTablePath) == false)
            {
                PrintHelp("Database MetaTable does not exist");
                return;
            }

            if (string.IsNullOrWhiteSpace(maxMemoryArg))
            {
                PrintHelp("--max-memory must be set");
                return;
            }

            long maxMemory = 0;
            if (long.TryParse(maxMemoryArg, out maxMemory) == false)
            {
                PrintHelp("--max-memory could not be parsed");
                return;
            }

            VideoFingerPrintDatabaseMetaTableWrapper metaTable = VideoFingerPrintDatabaseMetaTableLoader.Load(databaseMetaTablePath);
            var random = new Random();
            foreach (string databasePath in metaTable.DatabaseMetaTableEntries.Select(e => e.FileName))
            {
                VideoFingerPrintDatabaseWrapper database = VideoFingerPrintDatabaseLoader.Load(databasePath);
                int videoFingerPrintSampleCount = (int)Math.Round(database.VideoFingerPrints.Length / 3.0);
                IEnumerable<VideoFingerPrintWrapper> videoFingerPrints = from fingerPrint in database.VideoFingerPrints
                                                                         where random.Next() % 2 == 0
                                                                         select fingerPrint;

                foreach (VideoFingerPrintWrapper videoFingerPrint in videoFingerPrints.Take(videoFingerPrintSampleCount))
                {
                    VideoFingerPrintWrapper actualVideoFingerPrint = Video.VideoIndexer.IndexVideo(videoFingerPrint.FilePath, maxMemory);

                    if (Equals(videoFingerPrint, actualVideoFingerPrint) == false)
                    {
                        Console.WriteLine("{0} Fingerprint does not match", Path.GetFileName(videoFingerPrint.FilePath));
                    }
                }
            }
        }
        #endregion
        #region coalesce
        private static void ExecuteCoalesceMetatable(string[] args)
        {
            string databaseMetaTablePath = GetDatabaseMetaTable(args);
            if (string.IsNullOrWhiteSpace(databaseMetaTablePath))
            {
                PrintHelp("Database metatable path not provided");
                return;
            }

            DatabaseCoalescer.Coalesce(databaseMetaTablePath);
        }
        #endregion
        #region util functions
        private static string GetDatabaseMetaTable(string[] args)
        {
            return GetArgumentTuple(args, "--database-metatable");
        }

        private static string GetVideoPath(string[] args)
        {
            return GetArgumentTuple(args, "--video");
        }

        private static string GetPhotoPath(string[] args)
        {
            return GetArgumentTuple(args, "--photo");
        }

        private static string GetNumThreads(string[] args)
        {
            return GetArgumentTuple(args, "--threads");
        }

        private static string GetMaxMemory(string[] args)
        {
            return GetArgumentTuple(args, "--max-memory");
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

                if (string.Equals(arg, "--check-database"))
                {
                    return Mode.CHECK;
                }

                if (string.Equals(arg, "--coalesce-database"))
                {
                    return Mode.COALESCE;
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
            builder.AppendLine("Video Indexer v2.2");

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
                .Append('\t').Append("--video").Append('\t').Append('\t').Append("The video to index. If a directory is specified, the entire directory will be recursively indexed").AppendLine()
                .Append('\t').Append("--database-metatable").Append('\t').Append("The path to save the database metatable to. This will update existing database metatables").AppendLine()
                .Append('\t').Append("--threads").Append('\t').Append("The number of threads to use when indexing. Default is 1").AppendLine()
                .Append('\t').Append("--max-memory").Append('\t').Append("The maximum number of bytes to take up in the frame buffer").AppendLine()
                .AppendLine()
                .AppendLine("Search Related Commands")
                .Append('\t').Append("--search").Append('\t').Append("Search for similar frames using an image").AppendLine()
                .Append('\t').Append("--photo").Append('\t').Append('\t').Append("The path to the photo you want to search for").AppendLine()
                .Append('\t').Append("--database-metatable").Append('\t').Append("The path to the database metatable").AppendLine()
                .AppendLine()
                .AppendLine("Database Operations")
                .Append('\t').Append("--check-database").Append('\t').Append("Ensure that the images hash to the values in the database").AppendLine()
                .Append('\t').Append("--coalesce-database").Append('\t').Append("Coalesce databases").AppendLine();

            Console.Write(builder.ToString());
        }
        #endregion
        #endregion
    }
}
