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

namespace Core.DSA
{
    /// <summary>
    /// Calculates the 
    /// </summary>
    public static class DistanceCalculator
    {
        /// <summary>
        /// Calculates the hamming distance between two image fingerprints
        /// </summary>
        /// <param name="pHashcodeA">The first hash code</param>
        /// <param name="pHashCodeB">The second hash code</param>
        /// <returns>The hamming distance between two hashcodes</returns>
        public static int CalculateHammingDistance(ulong pHashcodeA, ulong pHashCodeB)
        {
            int numBits = 0;
            for (int i = 0; i < 64; i++)
            {
                ulong aBit = pHashcodeA & ((ulong)1) << i;
                ulong bBit = pHashCodeB & ((ulong)1) << i;

                if (aBit != bBit)
                {
                    numBits++;
                }
            }

            return numBits;
        }
    }
}
