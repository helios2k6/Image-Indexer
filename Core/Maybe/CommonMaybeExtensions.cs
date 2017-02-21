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

namespace Core.Maybe
{
    /// <summary>
    /// Common Maybe extensions
    /// </summary>
    internal static class CommonMaybeExtensions
    {
        /// <summary>
        /// Invokes an action on the wrapped reference, if it exists, and returns the same reference
        /// to the Maybe{T}
        /// </summary>
        /// <typeparam name="T">The type wrapped by this Maybe</typeparam>
        /// <param name="this">The Maybe{T}</param>
        /// <param name="action">The action to invoke on the underlying object</param>
        /// <returns>A reference to the same Maybe{T} that this references</returns>
        public static Maybe<T> Apply<T>(this Maybe<T> @this, Action<T> action)
        {
            return @this.Select(t =>
            {
                action.Invoke(t);
                return t;
            });
        }
    }
}
