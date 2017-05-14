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

using Core.Media;
using Core.Model.Wrappers;
using FrameIndexLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VideoIndexer.Video
{
    internal sealed class VideoIndexingExecutor : IDisposable
    {
        #region private classes
        private sealed class WorkItem
        {
            public int FrameNumber { get; set; }

            public WritableLockBitImage Frame { get; set; }
        }
        #endregion

        #region private fields
        private static readonly int DefaultWorkerCapacity = 2;

        private readonly BlockingCollection<WorkItem> _buffer;
        private readonly ConcurrentBag<FrameFingerPrintWrapper> _fingerPrints;
        private readonly Task[] _workerThreads;
        private readonly int _numThreads;
        private readonly long _maxCapacity;
        private readonly ManualResetEventSlim _capacityBarrier;

        private long _currentMemoryLevel;
        private bool _disposed;
        #endregion

        #region ctor
        public VideoIndexingExecutor(int numThreads, long maxCapacity)
        {
            _disposed = false;
            _numThreads = numThreads;
            _buffer = new BlockingCollection<WorkItem>();
            _fingerPrints = new ConcurrentBag<FrameFingerPrintWrapper>();
            _workerThreads = new Task[_numThreads];
            _maxCapacity = maxCapacity;
            _capacityBarrier = new ManualResetEventSlim(true);
            _currentMemoryLevel = 0;

            for (int i = 0; i < _numThreads; i++)
            {
                _workerThreads[i] = Task.Factory.StartNew(RunConsumer);
            }
        }

        public VideoIndexingExecutor(long maxCapacity) : this(DefaultWorkerCapacity, maxCapacity)
        {
        }
        #endregion

        #region public methods

        public void SubmitVideoFrame(WritableLockBitImage frame, int frameNumber)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }

            // Wait until capacity is available
            _capacityBarrier.Wait();

            Interlocked.Add(ref _currentMemoryLevel, frame.MemorySize);

            _buffer.Add(new WorkItem
            {
                FrameNumber = frameNumber,
                Frame = frame,
            });
        }

        public void Shutdown()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }

            _buffer.CompleteAdding();
        }

        public void Wait()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }

            Task.WaitAll(_workerThreads);
        }

        public IEnumerable<FrameFingerPrintWrapper> GetFingerPrints()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }

            return _fingerPrints;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _buffer.Dispose();
            _capacityBarrier.Dispose();
        }
        #endregion

        #region private methods
        private void RunConsumer()
        {
            try
            {
                foreach (WorkItem item in _buffer.GetConsumingEnumerable())
                {
                    using (WritableLockBitImage frame = item.Frame)
                    {
                        frame.Lock();
                        ulong framePHash = FrameIndexer.IndexFrame(item.Frame.GetImage());
                        _fingerPrints.Add(new FrameFingerPrintWrapper
                        {
                            PHashCode = framePHash,
                            FrameNumber = item.FrameNumber,
                        });

                        // Reduce the current memory level
                        long currentMemoryLevels = Interlocked.Add(ref _currentMemoryLevel, -frame.MemorySize);

                        // Check capacity
                        if (currentMemoryLevels < _maxCapacity)
                        {
                            // If we have space, set event
                            _capacityBarrier.Set();
                        }
                        else
                        {
                            // Otherwise, reset the event
                            _capacityBarrier.Reset();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Shutdown();
            }
        }
        #endregion
    }
}