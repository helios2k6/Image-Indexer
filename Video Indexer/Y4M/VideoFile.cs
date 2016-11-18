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

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Represents a Y4M Video File
    /// </summary>
    internal sealed class VideoFile
    {
        #region public properties
        /// <summary>
        /// The file header
        /// </summary>
        public Header Header { get; }

        /// <summary>
        /// The video frames
        /// </summary>
        public IEnumerable<VideoFrame> Frames { get; }

        /// <summary>
        /// The byte offsets that designate the beginning of each frame
        /// </summary>
        public IEnumerable<long> FrameOffsets { get; }

        /// <summary>
        /// The path to the original video file
        /// </summary>
        public string FilePath { get; }
        #endregion

        #region ctor
        /// <summary>
        /// Construct a new VideoFile
        /// </summary>
        /// <param name="header">The file level header</param>
        /// <param name="frames">The video frames</param>
        /// <param name="filePath">The path to the original video file</param>
        /// <param name="frameOffsets">The byte offsets for each frame</param>
        public VideoFile(Header header, IEnumerable<VideoFrame> frames, IEnumerable<long> frameOffsets, string filePath)
        {
            Header = header;
            Frames = frames;
            FrameOffsets = frameOffsets;
            FilePath = filePath;
        }
        #endregion
    }
}
