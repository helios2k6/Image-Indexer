﻿/* 
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
using System.Linq;

namespace Core.Model.Wrappers
{
    /// <summary>
    /// Represents the Image Finger Print database
    /// </summary>
    public sealed class VideoFingerPrintDatabaseWrapper : IEquatable<VideoFingerPrintDatabaseWrapper>
    {
        #region public properties
        /// <summary>
        /// The fingerprints of all of the videos
        /// </summary>
        public VideoFingerPrintWrapper[] VideoFingerPrints { get; set; }
        #endregion

        #region ctor
        /// <summary>
        /// Construct a default VideoFingerPrintDatabaseWrapper
        /// </summary>
        public VideoFingerPrintDatabaseWrapper()
        {
            VideoFingerPrints = new VideoFingerPrintWrapper[0];
        }
        #endregion

        #region public methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(VideoFingerPrintDatabaseWrapper other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return Enumerable.SequenceEqual(VideoFingerPrints, other.VideoFingerPrints);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (EqualsPreamble(obj) == false)
            {
                return false;
            }

            return Equals(obj as VideoFingerPrintDatabaseWrapper);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return VideoFingerPrints != null
                ? VideoFingerPrints.Aggregate(0, (acc, f) => acc ^ f.GetHashCode())
                : 0;
        }
        #endregion

        #region private methods
        private bool EqualsPreamble(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return true;
        }
        #endregion
    }
}