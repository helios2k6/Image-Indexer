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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VideoIndexer.BGR24
{
    internal sealed class BGR24VideoReader
    {
        #region private fields
        private readonly string _videoFile;
        private readonly int _width;
        private readonly int _height;
        private readonly int _numFrames;
        #endregion

        #region public properties
        public string VideoFile
        {
            get { return _videoFile; }
        }

        public int NumFrames
        {
            get { return _numFrames; }
        }
        #endregion

        #region ctor
        public BGR24VideoReader(string videoFile, int width, int height, int numFrames)
        {
            _videoFile = videoFile;
            _width = width;
            _height = height;
            _numFrames = numFrames;
        }
        #endregion

        #region public methods
        public WritableLockBitImage GetFrame(int frameNumber)
        {
            using (var stream = new FileStream(_videoFile, FileMode.Open, FileAccess.Read, FileShare.Read, 64000))
            {
                stream.Position = CalculateFrameOffset(frameNumber, _width, _height);
                var outputFrame = new WritableLockBitImage(_width, _height);
                int frameSize = 3 * _width * _height;
                byte[] frameBuffer = new byte[frameSize];
                int readBytes = stream.Read(frameBuffer, 0, frameSize);
                if (readBytes != frameSize)
                {
                    throw new InvalidDataException("Could not read frame");
                }

                outputFrame.SetFrame(frameBuffer);
                outputFrame.Lock();

                return outputFrame;
            }
        }
        #endregion

        #region private methods
        private static IEnumerable<long> CalculateOffsets(int numFrames, int width, int height)
        {
            return Enumerable.Range(0, numFrames).Select(frame => CalculateFrameOffset(frame, width, height));
        }

        private static long CalculateFrameOffset(int frameNumber, int width, int height)
        {
            return frameNumber * width * height * 3L;
        }
        #endregion
    }
}