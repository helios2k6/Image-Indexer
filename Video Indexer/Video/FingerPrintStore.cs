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

using Core.Model;
using Core.Model.Serialization;
using Core.Model.Wrappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VideoIndex.Video
{
    internal sealed class FingerPrintStore : IDisposable
    {
        #region private fields
        private static readonly ulong MaxDatabaseSize = 838860800; // 800 Mebibyte Database Limit

        private bool _disposed;
        private bool _shutdown;

        private readonly List<VideoFingerPrintWrapper> _fingerprints;
        private readonly BlockingCollection<VideoFingerPrintWrapper> _workItems;
        private readonly string _metatablePath;
        private readonly Task _queueTask;
        #endregion

        #region ctor
        public FingerPrintStore(string metatablePath)
        {
            _metatablePath = metatablePath;
            _fingerprints = new List<VideoFingerPrintWrapper>();
            _workItems = new BlockingCollection<VideoFingerPrintWrapper>();
            _queueTask = Task.Factory.StartNew(RunQueue);
            _disposed = false;
            _shutdown = false;
        }
        #endregion

        #region public methods
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_shutdown == false)
            {
                throw new InvalidOperationException("Cannot dispose of this object until Shutdown() has been called");
            }

            if (_queueTask.IsCompleted == false && _queueTask.IsFaulted == false && _queueTask.IsCanceled == false)
            {
                throw new InvalidOperationException("Cannot dispose of this object until the Task running the work queue has finished");
            }

            _disposed = true;
            _workItems.Dispose();
            _queueTask.Dispose();
        }

        public void AddFingerPrint(VideoFingerPrintWrapper fingerprint)
        {
            _workItems.Add(fingerprint);
        }

        public void Shutdown()
        {
            if (_shutdown)
            {
                return;
            }

            _shutdown = true;
            _workItems.CompleteAdding();
        }

        public void Wait()
        {
            _queueTask.Wait();
        }
        #endregion

        #region private methods
        private static VideoFingerPrintDatabaseMetaTableWrapper CreateOrLoadMetatable(string metatablePath)
        {
            if (File.Exists(metatablePath))
            {
                return VideoFingerPrintDatabaseMetaTableLoader.Load(metatablePath);
            }

            return new VideoFingerPrintDatabaseMetaTableWrapper();
        }

        private void RunQueue()
        {
            var fingerprintBuffer = new List<VideoFingerPrintWrapper>();
            Tuple<VideoFingerPrintDatabaseWrapper, string> currentDatabaseTuple = GetNextEligibleDatabase();
            bool needsFinalFlush = false;
            foreach (VideoFingerPrintWrapper fingerprint in _workItems.GetConsumingEnumerable())
            {
                try
                {
                    Console.WriteLine("Adding fingerprint: {0}", Path.GetFileName(fingerprint.FilePath));
                    fingerprintBuffer.Add(fingerprint);
                    if (fingerprintBuffer.Count > 5)
                    {
                        VideoFingerPrintDatabaseWrapper currentDatabase = currentDatabaseTuple.Item1;
                        string currentDatabasePath = currentDatabaseTuple.Item2;

                        Console.WriteLine("Flushing database");
                        // Flush the buffer
                        needsFinalFlush = false;

                        // Add entries to database
                        currentDatabase.VideoFingerPrints = currentDatabase.VideoFingerPrints.Concat(fingerprintBuffer).ToArray();

                        // Save entries to disk
                        VideoFingerPrintDatabaseSaver.Save(currentDatabase, currentDatabasePath);

                        FileInfo fileInfo = new FileInfo(currentDatabasePath);
                        ulong fileSize = (ulong)fileInfo.Length;

                        // Save metatable to disk
                        UpdateMetatable(currentDatabasePath, fileSize);

                        // Now, check if we need to update the current database
                        if (fileSize > MaxDatabaseSize)
                        {
                            currentDatabaseTuple = GetNextEligibleDatabase();
                        }

                        // Lastly, clear the buffer
                        fingerprintBuffer.Clear();
                    }
                    else
                    {
                        needsFinalFlush = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not write database. {0}", e.Message);
                }
            }

            if (needsFinalFlush)
            {
                try
                {
                    Console.WriteLine("Flushing database for the final time");
                    // Flush the buffer one last time
                    // Add entries to database
                    currentDatabaseTuple.Item1.VideoFingerPrints = currentDatabaseTuple.Item1.VideoFingerPrints.Concat(fingerprintBuffer).ToArray();

                    // Save entries to disk
                    VideoFingerPrintDatabaseSaver.Save(currentDatabaseTuple.Item1, currentDatabaseTuple.Item2);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not perform final flush of database. {0}", e.Message);
                }
            }
        }

        private void UpdateMetatable(string databasePath, ulong fileSize)
        {
            VideoFingerPrintDatabaseMetaTableWrapper metatable = CreateOrLoadMetatable(_metatablePath);

            // Get matching database entry
            VideoFingerPrintDatabaseMetaTableEntryWrapper databaseEntry = (from entry in metatable.DatabaseMetaTableEntries
                                                                           where string.Equals(entry.FileName, databasePath, StringComparison.Ordinal)
                                                                           select entry).SingleOrDefault();
            databaseEntry.FileSize = fileSize;

            VideoFingerPrintDatabaseMetaTableSaver.Save(metatable, _metatablePath);
        }

        private Tuple<VideoFingerPrintDatabaseWrapper, string> GetNextEligibleDatabase()
        {
            VideoFingerPrintDatabaseMetaTableWrapper metatable = CreateOrLoadMetatable(_metatablePath);
            Tuple<VideoFingerPrintDatabaseWrapper, string> eligibleDatabase = (from entry in metatable.DatabaseMetaTableEntries
                                                                               where entry.FileSize < MaxDatabaseSize
                                                                               select Tuple.Create(VideoFingerPrintDatabaseLoader.Load(entry.FileName), entry.FileName)).FirstOrDefault();

            // If we can't find an eligible database, then we need to create one and send it back
            return eligibleDatabase ?? CreateNewDatabaseAndAddToMetatable();
        }

        private Tuple<VideoFingerPrintDatabaseWrapper, string> CreateNewDatabaseAndAddToMetatable()
        {
            VideoFingerPrintDatabaseMetaTableWrapper metatable = CreateOrLoadMetatable(_metatablePath);
            string emptyDatabaseFileName = Path.GetRandomFileName() + ".bin";
            VideoFingerPrintDatabaseWrapper emptyDatabase = new VideoFingerPrintDatabaseWrapper();
            VideoFingerPrintDatabaseSaver.Save(emptyDatabase, emptyDatabaseFileName);

            // Add to the metatable
            var databaseEntries = new List<VideoFingerPrintDatabaseMetaTableEntryWrapper>(metatable.DatabaseMetaTableEntries);
            databaseEntries.Add(new VideoFingerPrintDatabaseMetaTableEntryWrapper
            {
                FileName = emptyDatabaseFileName,
                FileSize = 0ul,
            });

            metatable.DatabaseMetaTableEntries = databaseEntries.ToArray();

            VideoFingerPrintDatabaseMetaTableSaver.Save(metatable, _metatablePath);

            return Tuple.Create(emptyDatabase, emptyDatabaseFileName);
        }
        #endregion
    }
}