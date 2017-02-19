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
    internal sealed class BKTreeNode<T> where T : IMetric<T>
    {
        #region private fields
        private readonly IDictionary<int, BKTreeNode<T>> _children;
        private readonly T _data;
        #endregion

        #region public properties
        public T Data
        {
            get { return _data; }
        }
        #endregion

        #region ctor
        public BKTreeNode(T data)
        {
            _children = new Dictionary<int, BKTreeNode<T>>();
            _data = data;
        }
        #endregion

        #region public methods
        public void Add(T element)
        {
            int distance = _data.CalculateDistance(element);
            if (_children.ContainsKey(distance))
            {
                _children[distance].Add(element);
            }
            else
            {
                _children.Add(distance, new BKTreeNode<T>(element));
            }
        }

        public Tuple<T, int> FindClosestElement(IMetric<T> metric)
        {
            foreach (var e in Query(metric, int.MaxValue).OrderBy(e => e.Value))
            {
                return Tuple.Create(e.Key.Data, e.Value);
            }

            return null;
        }

        public IDictionary<BKTreeNode<T>, int> Query(IMetric<T> metric, int radius)
        {
            IDictionary<BKTreeNode<T>, int> results = new Dictionary<BKTreeNode<T>, int>();
            QueryHelper(metric, radius, results);
            return results;
        }
        #endregion

        #region private methods
        private void QueryHelper(IMetric<T> metric, int radius, IDictionary<BKTreeNode<T>, int> acc)
        {
            int distanceToTargetElement = metric.CalculateDistance(_data);
            if (distanceToTargetElement <= radius)
            {
                acc.Add(this, distanceToTargetElement);
            }

            int lowerBound = distanceToTargetElement - radius;
            int upperBound = distanceToTargetElement + radius;
            foreach (int distance in _children.Keys)
            {
                if (distance > lowerBound || distance <= upperBound)
                {
                    _children[distance].QueryHelper(metric, radius, acc);
                }
            }
        }
        #endregion
    }
}
