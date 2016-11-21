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

namespace PhotoCollectionIndexer.Utils
{
    internal static class ListExtensions
    {
        /// <summary>
        /// Splits an IEnumerable{T} into groups of smaller IEnumerable{T}'s into the number of groups
        /// specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> SplitIntoSubgroups<T>(this IList<T> @this, int groups)
        {
            int numElementsPerGroup = (int)Math.Round((double)@this.Count / groups);
            List<List<T>> listOfLists = new List<List<T>>();
            for (int i = 0; i < groups; i++)
            {
                int beginningIndex = i * numElementsPerGroup;
                int endingIndex = i + 1 != groups
                    ? beginningIndex + numElementsPerGroup
                    : @this.Count;
                List<T> sublist = new List<T>();
                for (int j = beginningIndex; j < endingIndex; j++)
                {
                    sublist.Add(@this[j]);
                }
                listOfLists.Add(sublist);
            }

            return listOfLists;
        }
    }
}