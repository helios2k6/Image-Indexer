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

using ImageIndexer;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoIndexer.Y4M;

namespace VideoIndexer.Video
{
    internal sealed class VideoIndexThreadPool
    {
        #region private classes
        private sealed class WorkItem
        {
            public int FrameNumber { get; set; }

            public VideoFrame Frame { get; set; }
        }
        #endregion

        #region private fields
        private static readonly int DEFAULT_WORKER_THREADS = 3;

        private readonly BlockingCollection<WorkItem> _buffer;
        private readonly ConcurrentBag<FrameFingerPrintWrapper> _fingerPrints;
        private readonly Task[] _workerThreads;
        #endregion

        #region ctor
        public VideoIndexThreadPool()
        {
            _buffer = new BlockingCollection<WorkItem>();
            _fingerPrints = new ConcurrentBag<FrameFingerPrintWrapper>();
            _workerThreads = new Task[DEFAULT_WORKER_THREADS];

            for (int i = 0; i < DEFAULT_WORKER_THREADS; i++)
            {
                _workerThreads[i] = Task.Factory.StartNew(RunConsumer);
            }
        }
        #endregion

        #region public methods
        public void SubmitVideoFrame(VideoFrame frame, int frameNumber)
        {
            _buffer.Add(new WorkItem
            {
                FrameNumber = frameNumber,
                Frame = frame,
            });
        }

        public void Shutdown()
        {
            _buffer.CompleteAdding();
        }

        public void Wait()
        {
            Task.WaitAll(_workerThreads);
        }

        public IEnumerable<FrameFingerPrintWrapper> GetFingerPrints()
        {
            return _fingerPrints;
        }
        #endregion

        #region private methods
        private void RunConsumer()
        {
            foreach (WorkItem item in _buffer.GetConsumingEnumerable())
            {
                FrameFingerPrintWrapper fingerPrint = Indexer.IndexFrame(item.Frame.LockBitImage.GetImage(), item.FrameNumber);
                _fingerPrints.Add(fingerPrint);
            }
        }
        #endregion
    }
}