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

using System;
using System.Linq;
using YAXLib;

namespace Core.Media
{
    /// <summary>
    /// Represents the File child node
    /// </summary>
    [Serializable]
    [YAXSerializeAs("File")]
    public sealed class FileXMLNode : IEquatable<FileXMLNode>
    {
        #region ctor
        public FileXMLNode()
        {
            Tracks = new Track[0];
        }
        #endregion

        #region public properties
        /// <summary>
        /// The media tracks of this media file
        /// </summary>
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        [YAXCollection(YAXCollectionSerializationTypes.RecursiveWithNoContainingElement, EachElementName = "track")]
        public Track[] Tracks { get; set; }
        #endregion

        #region public methods
        public bool Equals(FileXMLNode other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return Enumerable.SequenceEqual(Tracks, other.Tracks);
        }

        public override bool Equals(object other)
        {
            return Equals(other as FileXMLNode);
        }

        public override int GetHashCode()
        {
            return Tracks.Aggregate(0, (acc, t) => acc ^ t.GetHashCode());
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
