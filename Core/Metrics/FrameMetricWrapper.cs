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

using Core.DSA;
using Core.Model.Wrappers;

namespace Core.Metrics
{
    /// <summary>
    /// A metric wrapper around a Frame Finger Print that allows it to be used with the BKTree
    /// </summary>
    public sealed class FrameMetricWrapper : IMetric<FrameFingerPrintWrapper>, IMetric<PhotoFingerPrintWrapper>, IMetric<FrameMetricWrapper>
    {
        public FrameFingerPrintWrapper Frame { get; set; }

        public VideoFingerPrintWrapper Video { get; set; }

        public int CalculateDistance(FrameMetricWrapper other)
        {
            return Frame.CalculateDistance(other.Frame);
        }

        public int CalculateDistance(PhotoFingerPrintWrapper other)
        {
            return Frame.CalculateDistance(other);
        }

        public int CalculateDistance(FrameFingerPrintWrapper other)
        {
            return Frame.CalculateDistance(other);
        }
    }
}