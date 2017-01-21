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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoIndexer.Serialization;
using VideoIndexer.Wrappers;

namespace VideoIndex
{
    internal sealed class FingerPrintStore
    {
        #region private fields
        private readonly List<VideoFingerPrintWrapper> _fingerprints;
        private readonly BlockingCollection<VideoFingerPrintWrapper> _workItems;
        private readonly string _databaseFilePath;
        private readonly Task _queueTask;
        #endregion

        #region ctor
        public FingerPrintStore(string databaseFilePath)
        {
            _databaseFilePath = databaseFilePath;
            _fingerprints = new List<VideoFingerPrintWrapper>();
            _workItems = new BlockingCollection<VideoFingerPrintWrapper>();
            _queueTask = Task.Factory.StartNew(RunQueue);
        }
        #endregion

        #region public methods
        public void AddFingerprint(VideoFingerPrintWrapper fingerprint)
        {
            _workItems.Add(fingerprint);
        }

        public void Shutdown()
        {
            _workItems.CompleteAdding();
        }

        public void Wait()
        {
            _queueTask.Wait();
        }
        #endregion

        #region private methods
        private void RunQueue()
        {
            bool needsFinalFlush = false;
            foreach (VideoFingerPrintWrapper fingerprint in _workItems.GetConsumingEnumerable())
            {
                try
                {
                    Console.WriteLine("Adding {0} to database", fingerprint.FilePath);
                    needsFinalFlush = true;
                    _fingerprints.Add(fingerprint);
                    if (_fingerprints.Count % 5 == 0)
                    {
                        Console.WriteLine("Flushing database");
                        // Flush
                        _database.VideoFingerPrints = _fingerprints.ToArray();
                        DatabaseSaver.Save(_database, _databaseFilePath);
                        needsFinalFlush = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not write {0} to database. Reason: {1}", fingerprint.FilePath, e.Message);
                }
            }

            if (needsFinalFlush)
            {
                _database.VideoFingerPrints = _fingerprints.ToArray();
                DatabaseSaver.Save(_database, _databaseFilePath);
            }
        }
        #endregion
    }
}