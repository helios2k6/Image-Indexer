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
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VideoIndexer.Video
{
    internal sealed class RawByteStore : IDisposable
    {
        #region private fields
        private bool _disposed;
        private readonly int _width;
        private readonly int _height;
        private readonly BlockingCollection<byte[]> _rawByteQueue;
        private readonly VideoIndexingExecutor _videoIndexer;
        private readonly Task _queueTask;
        private readonly CancellationToken _cancellationToken;
        #endregion

        #region public properties
        #endregion

        #region ctor
        public RawByteStore(int width, int height, VideoIndexingExecutor videoIndexer, CancellationToken cancellationToken)
        {
            _disposed = false;
            _width = width;
            _height = height;
            _cancellationToken = cancellationToken;
            _rawByteQueue = new BlockingCollection<byte[]>();
            _videoIndexer = videoIndexer;
            _queueTask = Task.Factory.StartNew(RunQueue);
        }
        #endregion

        #region public methods
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _rawByteQueue.Dispose();
        }

        public void Submit(byte[] rawBytes, int bytesToCopy)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }

            byte[] copy = new byte[bytesToCopy];
            Buffer.BlockCopy(rawBytes, 0, copy, 0, bytesToCopy);
            _rawByteQueue.Add(copy);
        }

        public void Shutdown()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }

            _rawByteQueue.CompleteAdding();
        }

        public void Wait()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }

            _queueTask.Wait();
        }
        #endregion

        #region private methods
        private void RunQueue()
        {
            int currentFrame = 0;
            int frameSize = 3 * _width * _height;
            byte[] frameBuffer = new byte[frameSize];
            int currentIndex = 0;
            try
            {
                foreach (byte[] rawBytes in _rawByteQueue.GetConsumingEnumerable(_cancellationToken))
                {
                    // Raw bytes don't overflow frame
                    if (rawBytes.Length < frameSize - currentIndex)
                    {
                        Buffer.BlockCopy(rawBytes, 0, frameBuffer, currentIndex, rawBytes.Length);
                        currentIndex += rawBytes.Length;
                    }
                    // Raw bytes overflow frame
                    else
                    {
                        int numBytesToCopy = frameSize - currentIndex;
                        Buffer.BlockCopy(rawBytes, 0, frameBuffer, currentIndex, numBytesToCopy);

                        // Frame is now full. Create image and ship it off
                        WritableLockBitImage frame = new WritableLockBitImage(_width, _height, frameBuffer);
                        frame.Lock();

                        _videoIndexer.SubmitVideoFrame(frame, currentFrame);
                        currentFrame++;

                        // Write overflow stuff now
                        Buffer.BlockCopy(rawBytes, numBytesToCopy, frameBuffer, 0, rawBytes.Length - numBytesToCopy);
                        currentIndex = rawBytes.Length - numBytesToCopy;
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
