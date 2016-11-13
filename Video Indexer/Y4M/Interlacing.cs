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
using System;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// Represents the interlacing method of the Y4M video
    /// </summary>
    public sealed class Interlacing : IEquatable<Interlacing>
    {
        #region public fields
        /// <summary>
        /// Signals a progressive video frame
        /// </summary>
        public static readonly Interlacing PROGRESSIVE = new Interlacing("Progressive", 0, "p");

        /// <summary>
        /// Signals an interlaced frame with the top field first
        /// </summary>
        public static readonly Interlacing TOP_FIELD_FIRST = new Interlacing("Top Field First", 1, "t");

        /// <summary>
        /// Signals an interlaced frame with the bottom field first
        /// </summary>
        public static readonly Interlacing BOTTOM_FIELD_FIRST = new Interlacing("Bottom Field First", 2, "b");

        /// <summary>
        /// Signals a mix of progressive video frames and/or interlaced frames with top or bottom field first. 
        /// Each frame will specify which field comes first or whether it's progressive
        /// </summary>
        public static readonly Interlacing MIXED = new Interlacing("Mixed", 3, "m");
        #endregion

        #region public properties
        /// <summary>
        /// The parameter argument used when parsing
        /// </summary>
        public string ParameterArgument { get; }

        /// <summary>
        /// The name of interlacing method
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The canonical value given to this interlacing method
        /// </summary>
        public int Value { get; }
        #endregion

        #region ctor
        private Interlacing(string name, int value, string parameterArgument)
        {
            Name = name;
            Value = value;
            ParameterArgument = parameterArgument;
        }
        #endregion

        #region public methods
        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Interlacing other)
        {
            return ReferenceEquals(this, other);
        }

        public override bool Equals(object other)
        {
            return Equals(other as Interlacing);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        /// <summary>
        /// Attempt to parse the string parameter as an Interlacing type
        /// </summary>
        /// <param name="parameter">The frame or file level header parameter</param>
        /// <returns>An optional type with the Interlacing if parsing was successful. None otherwise</returns>
        public static Maybe<Interlacing> TryParseInterlacing(string parameter)
        {
            if (string.Equals(parameter, PROGRESSIVE.Name, StringComparison.OrdinalIgnoreCase))
            {
                return PROGRESSIVE.ToMaybe();
            }
            else if (string.Equals(parameter, TOP_FIELD_FIRST.Name, StringComparison.OrdinalIgnoreCase))
            {
                return TOP_FIELD_FIRST.ToMaybe();
            }
            else if (string.Equals(parameter, BOTTOM_FIELD_FIRST.Name, StringComparison.OrdinalIgnoreCase))
            {
                return BOTTOM_FIELD_FIRST.ToMaybe();
            }
            else if (string.Equals(parameter, MIXED.Name, StringComparison.OrdinalIgnoreCase))
            {
                return MIXED.ToMaybe();
            }

            return Maybe<Interlacing>.Nothing;
        }
        #endregion
    }
}
