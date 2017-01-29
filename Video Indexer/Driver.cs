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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoIndex;
using VideoIndexer.Serialization;
using VideoIndexer.Utils;
using VideoIndexer.Wrappers;

namespace VideoIndexer
{
    internal static class Driver
    {
        private static readonly CancellationTokenSource PanicButton = new CancellationTokenSource();
        private static int CancelRequestedCount = 0;

        internal enum Mode
        {
            INDEX,
            SEARCH,
            MERGE,
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

            Console.CancelKeyPress += ConsoleCancelKeyPress;

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

            if (mode == Mode.MERGE)
            {
                ExecuteMerge(args);
            }
        }

        private static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            int incrementedValue = Interlocked.Increment(ref CancelRequestedCount);

            if (incrementedValue < 1)
            {
                e.Cancel = true;
                Console.WriteLine("Cancellation requested. Shutting down systems cleanly");
                PanicButton.Cancel();
            }
            else
            {
                Console.WriteLine("Cancellation forced! Shutting down systems immediately");
            }
        }
        #endregion

        #region private methods
        private static void ExecuteMerge(string[] args)
        {
            Tuple<string, string, string> databasesToMerge = GetMergeArguments(args);
            if (string.IsNullOrWhiteSpace(databasesToMerge.Item1) || string.IsNullOrWhiteSpace(databasesToMerge.Item2) || string.IsNullOrWhiteSpace(databasesToMerge.Item3))
            {
                Console.WriteLine("Database files not provided");
                return;
            }

            if (File.Exists(databasesToMerge.Item1) == false || File.Exists(databasesToMerge.Item2) == false)
            {
                Console.WriteLine("Database files cannot be found");
                return;
            }

            if (File.Exists(databasesToMerge.Item3))
            {
                Console.WriteLine(string.Format("Database {0} exists. Cannot override", databasesToMerge.Item3));
                return;
            }

            VideoFingerPrintDatabaseWrapper mergedDatabase = DatabaseMerger.Merge(
                DatabaseLoader.Load(databasesToMerge.Item1),
                DatabaseLoader.Load(databasesToMerge.Item2)
            );

            DatabaseSaver.Save(mergedDatabase, databasesToMerge.Item3);
        }

        private static void ExecuteIndex(string[] args)
        {
            string videoFile = GetVideoPath(args);
            string databaseFile = GetDatabaseMetaTable(args);
            string numThreadsArg = GetNumThreads(args);
            string maxMemoryArg = GetMaxMemory(args);

            if (string.IsNullOrWhiteSpace(videoFile) || string.IsNullOrWhiteSpace(databaseFile))
            {
                PrintHelp("Video path or database path not provided");
                return;
            }

            int numThreads = 1;
            if (string.IsNullOrWhiteSpace(numThreadsArg) == false)
            {
                int.TryParse(numThreadsArg, out numThreads);
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

            if (maxMemory <= 0)
            {
                PrintHelp("--max-memory must be greater than 0");
                return;
            }

            IndexImpl(databaseFile, GetVideoFiles(videoFile), numThreads, maxMemory);
        }

        private static void IndexImpl(string databaseMetaTablePath, IEnumerable<string> videoPaths, int numThreads, long maxMemory)
        {
            IEnumerable<string> knownHashes = GetKnownDatabaseEntries(databaseMetaTablePath);
            var skippedFiles = new ConcurrentBag<string>();

            using (ChangeErrorMode _ = new ChangeErrorMode(ChangeErrorMode.ErrorModes.FailCriticalErrors | ChangeErrorMode.ErrorModes.NoGpFaultErrorBox))
            using (FingerPrintStore store = new FingerPrintStore(databaseMetaTablePath))
            {
                Parallel.ForEach(
                    videoPaths.Except(knownHashes),
                    new ParallelOptions { MaxDegreeOfParallelism = numThreads },
                    videoPath =>
                    {
                        if (PanicButton.IsCancellationRequested)
                        {
                            return;
                        }

                        string fileName = Path.GetFileName(videoPath);
                        try
                        {
                            if (videoPath.Length > 260) // Max file path that mediainfo can handle
                            {
                                Console.WriteLine("File path is too long. Skipping: " + fileName);
                                return;
                            }

                            VideoFingerPrintWrapper videoFingerPrint = Video.VideoIndexer.IndexVideo(videoPath, PanicButton.Token, maxMemory);
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
                        VideoFingerPrintWrapper videoFingerPrint = Video.VideoIndexer.IndexVideo(skippedFile, PanicButton.Token, maxMemory);
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
            DatabaseMetaTableWrapper metatable = DatabaseMetaTableLoader.Load(databaseMetaTablePath);
            foreach (DatabaseMetaTableEntryWrapper entry in metatable.DatabaseMetaTableEntries)
            {
                VideoFingerPrintDatabaseWrapper database = DatabaseLoader.Load(entry.FileName);
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

            using (WritableLockBitImage lockbitImage = new WritableLockBitImage(Image.FromFile(photoFilePath), false))
            {
                lockbitImage.Lock();
                ulong providedPhotoHash = FrameIndexer.IndexFrame(lockbitImage);
                DatabaseMetaTableWrapper metaTable = DatabaseMetaTableLoader.Load(databaseMetaTablePath);
                foreach (string databasePath in metaTable.DatabaseMetaTableEntries.Select(e => e.FileName))
                {
                    VideoFingerPrintDatabaseWrapper database = DatabaseLoader.Load(databasePath);
                    Dictionary<int, HashSet<Tuple<string, int>>> distanceToFingerprints = new Dictionary<int, HashSet<Tuple<string, int>>>();
                    foreach (var video in database.VideoFingerPrints)
                    {
                        foreach (var fingerPrint in video.FingerPrints)
                        {
                            int distance = DistanceCalculator.CalculateHammingDistance(providedPhotoHash, fingerPrint.PHashCode);

                            HashSet<Tuple<string, int>> bucket;
                            if (distanceToFingerprints.TryGetValue(distance, out bucket) == false)
                            {
                                bucket = new HashSet<Tuple<string, int>>();
                                distanceToFingerprints.Add(distance, bucket);
                            }

                            bucket.Add(Tuple.Create(video.FilePath, fingerPrint.FrameNumber));
                        }
                    }

                    var filteredDistances = from distanceToBucket in distanceToFingerprints
                                            where distanceToBucket.Key <= 4
                                            orderby distanceToBucket.Key
                                            select distanceToBucket;

                    foreach (KeyValuePair<int, HashSet<Tuple<string, int>>> kvp in filteredDistances)
                    {
                        int distance = kvp.Key;
                        foreach (Tuple<string, int> entry in kvp.Value)
                        {
                            Console.WriteLine(string.Format("Distance {0} for {1} at Frame {2}", distance, entry.Item1, entry.Item2));
                        }
                    }
                }
            }
        }

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

        private static Tuple<string, string, string> GetMergeArguments(string[] args)
        {
            string first = null, second = null, output = null;
            bool collectStrings = false;
            foreach (string arg in args)
            {
                if (string.Equals("--merge", arg) && collectStrings == false)
                {
                    collectStrings = true;
                }
                else if (collectStrings)
                {
                    if (first == null)
                    {
                        first = arg;
                    }
                    else if (second == null)
                    {
                        second = arg;
                    }
                    else if (output == null)
                    {
                        output = arg;
                        break;
                    }
                }
            }

            if (first != null && second != null && output != null)
            {
                return Tuple.Create(first, second, output);
            }

            return Tuple.Create(string.Empty, string.Empty, string.Empty);
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

                if (string.Equals(arg, "--merge"))
                {
                    return Mode.MERGE;
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
            builder.AppendLine("Video Indexer v2.0");

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
                .Append('\t').Append("--merge").Append('\t').Append("Merge two databases into a third database").AppendLine();

            Console.Write(builder.ToString());
        }
        #endregion
    }
}
