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
using VideoIndexer.Wrappers;

namespace VideoIndex
{
    internal sealed class FingerPrintStore
    {
        #region private fields
        private string _currentDatabase;

        private readonly List<VideoFingerPrintWrapper> _fingerprints;
        private readonly BlockingCollection<VideoFingerPrintWrapper> _workItems;
        private readonly string _databaseMetaTablePath;
        private readonly Task _queueTask;
        #endregion

        #region ctor
        public FingerPrintStore(string databaseMetaTablePath)
        {
            _databaseMetaTablePath = databaseMetaTablePath;
            _fingerprints = new List<VideoFingerPrintWrapper>();
            _workItems = new BlockingCollection<VideoFingerPrintWrapper>();
            _queueTask = Task.Factory.StartNew(RunQueue);
            _currentDatabase = string.Empty;
        }
        #endregion

        #region public methods
        public void AddFingerPrint(VideoFingerPrintWrapper fingerprint)
        {
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

        }

        private void HandleAddingFingerPrint()
        {
        }
        #endregion
    }
}