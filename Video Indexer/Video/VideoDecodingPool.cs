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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoIndexer.BGR24;

namespace VideoIndexer.Video
{
    internal static class VideoDecodingPool
    {
        #region private fields
        private static readonly int DEFAULT_WORKER_THREADS = 3;
        #endregion

        #region public methods
        public static void StartDecoding(BGR24VideoReader videoFileReader, VideoIndexingExecutor sink, int currentFrameIndex)
        {
            StartDecoding(videoFileReader, sink, currentFrameIndex, DEFAULT_WORKER_THREADS);
        }

        public static void StartDecoding(BGR24VideoReader videoFileReader, VideoIndexingExecutor sink, int currentFrameIndex, int numThreads)
        {
            var workerTasks = new Task[numThreads];
            int lengthOfSubarrays = videoFileReader.NumFrames / numThreads;
            int remainder = videoFileReader.NumFrames % numThreads;
            for (int i = 0; i < numThreads; i++)
            {
                int firstElementIndex = i * lengthOfSubarrays;
                IEnumerable<int> slicedArray = remainder > 0 && i + 1 == numThreads
                    ? Enumerable.Range(firstElementIndex, lengthOfSubarrays + remainder)
                    : Enumerable.Range(firstElementIndex, lengthOfSubarrays);

                workerTasks[i] = Task.Factory.StartNew(() =>
                {
                    RunDecoderThread(videoFileReader, slicedArray, sink, firstElementIndex + currentFrameIndex);
                });
            }

            Task.WaitAll(workerTasks);
        }
        #endregion

        #region private methods
        private static void RunDecoderThread(
            BGR24VideoReader reader,
            IEnumerable<int> framesToDecode,
            VideoIndexingExecutor sink,
            int currentFrameIndex
        )
        {
            int localFrameIndex = 0;
            foreach (int frame in framesToDecode)
            {
                sink.SubmitVideoFrame(reader.GetFrame(frame), currentFrameIndex + localFrameIndex);
                localFrameIndex++;
            }
        }
        #endregion
    }
}