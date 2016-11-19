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

using Functional.Maybe;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoIndexer.Utils;
using VideoIndexer.Y4M;

namespace VideoIndexer.Video
{
    internal static class VideoDecodingPool
    {
        #region private fields
        private static readonly int DEFAULT_WORKER_THREADS = 3;
        #endregion

        #region public methods
        public static void StartDecoding(VideoFile videoFile, VideoIndexThreadPool sink, int currentFrameIndex)
        {
            var workerTasks = new Task[DEFAULT_WORKER_THREADS];
            List<long> offsets = videoFile.FrameOffsets.ToList();
            int lengthOfSubarrays = offsets.Count / DEFAULT_WORKER_THREADS;
            int remainder = offsets.Count % DEFAULT_WORKER_THREADS;
            for (int i = 0; i < DEFAULT_WORKER_THREADS; i++)
            {
                int firstElementIndex = i * lengthOfSubarrays;
                IEnumerable<long> slicedArray = remainder > 0 && i + 1 == DEFAULT_WORKER_THREADS
                    ? Slice(offsets, firstElementIndex, lengthOfSubarrays + remainder)
                    : Slice(offsets, firstElementIndex, lengthOfSubarrays);

                workerTasks[i] = Task.Factory.StartNew(() =>
                {
                    RunDecoderThread(videoFile.FilePath, videoFile.Header, slicedArray, sink, currentFrameIndex + firstElementIndex);
                });
            }

            Task.WaitAll(workerTasks);
        }
        #endregion

        #region private methods
        private static IEnumerable<long> Slice(List<long> input, int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return input[i + startIndex];
            }
        }

        private static void RunDecoderThread(
            string filePath,
            Header header,
            IEnumerable<long> offsets,
            VideoIndexThreadPool sink,
            int currentFrameIndex
        )
        {
            var parser = new VideoFrameParser(header);
            int localFrameIndex = 0;
            foreach (long offset in offsets)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Position = offset;
                    Maybe<VideoFrame> videoFrame = parser.TryParseVideoFrame(fileStream);
                    videoFrame.Apply(frame =>
                    {
                        sink.SubmitVideoFrame(frame, localFrameIndex + currentFrameIndex);
                        localFrameIndex++;
                    });
                }
            }
        }
        #endregion
    }
}