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
using FrameIndexLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PhotoCollectionIndexer.Executors
{
    internal sealed class PhotoFileIndexerExecutor
    {
        #region private fields
        private readonly PhotoFileReaderExecutor _fileReader;
        private readonly ConcurrentBag<PhotoFingerPrintWrapper> _fingerPrints;
        private readonly int _numWorkers;

        private bool _started;
        #endregion

        #region ctor
        public PhotoFileIndexerExecutor(PhotoFileReaderExecutor fileReader, int numWorkers)
        {
            _fileReader = fileReader;
            _fingerPrints = new ConcurrentBag<PhotoFingerPrintWrapper>();
            _numWorkers = numWorkers;
            _started = false;
        }
        #endregion

        #region public methods
        public Task Start()
        {
            if (_started)
            {
                throw new InvalidOperationException("Cannot start indexer twice");
            }

            Task[] workers = new Task[_numWorkers];
            _started = true;
            for (int i = 0; i < _numWorkers; i++)
            {
                workers[i] = Task.Factory.StartNew(RunIndexer);
            }

            return Task.WhenAll(workers);
        }

        public IEnumerable<PhotoFingerPrintWrapper> GetFingerPrints()
        {
            return _fingerPrints;
        }
        #endregion

        #region private methods
        private void RunIndexer()
        {
            foreach (Tuple<WritableLockBitImage, string> imageTuple in _fileReader.GetConsumingEnumerable())
            {
                using (imageTuple.Item1)
                {
                    PhotoFingerPrintWrapper fingerPrint = new PhotoFingerPrintWrapper
                    {
                        FilePath = Path.GetFullPath(imageTuple.Item2),
                        PHash = FrameIndexer.IndexFrame(imageTuple.Item1),
                    };

                    _fingerPrints.Add(fingerPrint);
                }
            }
        }
        #endregion
    }
}