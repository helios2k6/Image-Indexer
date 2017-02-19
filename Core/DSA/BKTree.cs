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

namespace Core.DSA
{
    /// <summary>
    /// Represents a BK Tree with an underlying data type of T
    /// </summary>
    /// <typeparam name="T">The underlying data type</typeparam>
    public sealed class BKTree<T> where T : IMetric<T>
    {
        #region private fields
        private BKTreeNode<T> _root = null;
        #endregion

        #region public methods
        /// <summary>
        /// Add an element to the tree
        /// </summary>
        /// <param name="element">The element</param>
        public void Add(T element)
        {
            if (_root != null)
            {
                _root.Add(element);
            }
            else
            {
                _root = new BKTreeNode<T>(element);
            }
        }

        /// <summary>
        /// Query an element with a given distance threshold
        /// </summary>
        /// <param name="metric">The metric to use to query nodes</param>
        /// <param name="threshold">The distance threshold</param>
        /// <returns>A dictionary of results that are within the distance threshold</returns>
        public IDictionary<T, int> Query(IMetric<T> metric, int threshold)
        {
            return _root.Query(metric, threshold).ToDictionary(e => e.Key.Data, e => e.Value);
        }

        /// <summary>
        /// Find the best node that is closest to the given element
        /// </summary>
        /// <param name="metric">The metric to use to find the closest neighbor</param>
        /// <returns>A tuple with the closest neighbor</returns>
        public Tuple<T, int> FindClosestElement(IMetric<T> metric)
        {
            return _root.FindClosestElement(metric);
        }
        #endregion
    }
}
