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
using PhotoCollectionIndexer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoCollectionIndexer.Executors
{
    internal sealed class PhotoFileReaderExecutor
    {
        #region private fields
        private static readonly int DEFAULT_NUM_WORKERS = 2;

        private readonly IEnumerable<string> _photoPaths;
        private readonly int _numWorkers;
        private readonly BlockingCollection<Tuple<WritableLockBitImage, string>> _loadedImages;

        private bool _started;
        #endregion

        #region ctor
        /// <summary>
        /// Construct a new photo file executor
        /// </summary>
        /// <param name="photoPaths">The photos to load into memory</param>
        /// <param name="numWorkers">The number of worker threads to use</param>
        public PhotoFileReaderExecutor(IEnumerable<string> photoPaths, int numWorkers)
        {
            _numWorkers = numWorkers;
            _photoPaths = photoPaths;
            _loadedImages = new BlockingCollection<Tuple<WritableLockBitImage, string>>();
            _started = false;
        }

        public PhotoFileReaderExecutor(IEnumerable<string> photoPaths)
            : this(photoPaths, DEFAULT_NUM_WORKERS)
        {
        }
        #endregion

        #region public methods
        public IEnumerable<Tuple<WritableLockBitImage, string>> GetConsumingEnumerable()
        {
            return _loadedImages.GetConsumingEnumerable();
        }

        public Task LoadPhotos()
        {
            if (_started)
            {
                throw new InvalidOperationException("Already started loading photos");
            }
            _started = true;

            Task[] workerTasks = new Task[_numWorkers];
            List<string> allPhotoPaths = _photoPaths.ToList();
            IEnumerable<IEnumerable<string>> sublistOfPhotos = allPhotoPaths.SplitIntoSubgroups(_numWorkers);
            int workerIndex = 0;
            foreach (IEnumerable<string> sublist in sublistOfPhotos)
            {
                workerTasks[workerIndex] = StartReaderThread(sublist);
                workerIndex++;
            }

            return Task.WhenAll(workerTasks).ContinueWith(_ => _loadedImages.CompleteAdding());
        }
        #endregion

        #region private methods
        private Task StartReaderThread(IEnumerable<string> photoPaths)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (string path in photoPaths)
                {
                    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64000))
                    {
                        _loadedImages.Add(
                            Tuple.Create(
                                new WritableLockBitImage(Image.FromStream(fileStream), false, true),
                                path
                            )
                        );
                    }
                }
            });
        }
        #endregion
    }
}