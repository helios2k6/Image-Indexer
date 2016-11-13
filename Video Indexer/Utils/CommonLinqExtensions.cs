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
using System.Collections.Generic;
using System.Linq;

namespace VideoIndexer.Utils
{
    public static class CommonLinqExtensions
    {
        /// <summary>
        /// Append an item to the end of an existing IEnumerable{T}
        /// </summary>
        /// <typeparam name="T">The type of item</typeparam>
        /// <param name="this">The IEnumerable to append to</param>
        /// <param name="item">The item to apend</param>
        /// <returns>A new IEnumerable{T} with the item appended</returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> @this, T item)
        {
            foreach (var i in @this)
            {
                yield return i;
            }

            yield return item;
        }

        /// <summary>
        /// Select the first IEnumerable{T} if it has elements, else select the second
        /// </summary>
        /// <typeparam name="T">The type inside the IEnumerable</typeparam>
        /// <param name="first">The first IEnumerable to check</param>
        /// <param name="fallback">The fallback to execute if there are no elements in the first</param>
        /// <returns>The first IEnumerable if it has elements, else the second</returns>
        public static IEnumerable<T> Or<T>(this IEnumerable<T> first, IEnumerable<T> fallback)
        {
            if (first.Any())
            {
                return first;
            }

            return fallback;
        }

        /// <summary>
        /// Invokes the action on this IEnumerable{T} if it isn't empty and returns its result or a default result
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="this">This IEnumerable{T}</param>
        /// <param name="action">The action to take on this IEnumerable{T}</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>An IEnumerable{T} with the action applied or the default value</returns>
        public static K IfNotEmpty<T, K>(this IEnumerable<T> @this, Func<IEnumerable<T>, K> action, K defaultValue)
        {
            if (@this.Any())
            {
                return action.Invoke(@this);
            }

            return defaultValue;
        }
    }
}
