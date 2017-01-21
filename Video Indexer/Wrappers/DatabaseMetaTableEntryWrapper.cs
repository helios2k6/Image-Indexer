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

namespace VideoIndexer.Wrappers
{
    /// <summary>
    /// Represents a Database MetaTable Entry
    /// </summary>
    public sealed class DatabaseMetaTableEntryWrapper : IEquatable<DatabaseMetaTableEntryWrapper>
    {
        #region public properties
        /// <summary>
        /// The file name of the database file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The size of the database in bytes
        /// </summary>
        public ulong FileSize { get; set; }
        #endregion

        #region public methods
        public bool Equals(DatabaseMetaTableEntryWrapper other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return string.Equals(FileName, other.FileName, StringComparison.Ordinal) &&
                long.Equals(FileSize, other.FileSize);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DatabaseMetaTableEntryWrapper);
        }

        public override int GetHashCode()
        {
            return (FileName ?? string.Empty).GetHashCode() ^
                FileSize.GetHashCode();
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
