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

        public Tuple<T, int> FindBestMatch(T element, int bestDistance)
        {
            T bestElement;
            int distance = FindBestMatchHelper(element, bestDistance, out bestElement);
            return Tuple.Create(bestElement, distance);
        }

        public IDictionary<BKTreeNode<T>, int> Query(T element, int threshold)
        {
            IDictionary<BKTreeNode<T>, int> results = new Dictionary<BKTreeNode<T>, int>();
            QueryHelper(element, threshold, results);
            return results;
        }
        #endregion

        #region private methods
        private int FindBestMatchHelper(T element, int bestDistance, out T bestElement)
        {
            int distanceAtNode = _data.CalculateDistance(element);

            bestElement = element;

            if (distanceAtNode < bestDistance)
            {
                bestDistance = distanceAtNode;
                bestElement = _data;
            }

            int possibleBest = bestDistance;

            foreach (int distance in _children.Keys)
            {
                if (distance < distanceAtNode + bestDistance)
                {
                    possibleBest = _children[distance].FindBestMatchHelper(element, bestDistance, out bestElement);
                    if (possibleBest < bestDistance)
                    {
                        bestDistance = possibleBest;
                    }
                }
            }

            return bestDistance;
        }

        private void QueryHelper(T element, int threshold, IDictionary<BKTreeNode<T>, int> acc)
        {
            int distanceAtNode = _data.CalculateDistance(element);
            if (distanceAtNode == threshold)
            {
                acc.Add(this, distanceAtNode);
                return;
            }

            if (distanceAtNode < threshold)
            {
                acc.Add(this, distanceAtNode);
            }

            for (int distance = (distanceAtNode - threshold); distance <= (threshold + distanceAtNode); distance++)
            {
                if (_children.ContainsKey(distance))
                {
                    _children[distance].QueryHelper(element, threshold, acc);
                }
            }
        }
        #endregion
    }
}
