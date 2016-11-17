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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace ImageIndexer
{
    /// <summary>
    /// A fast DCT calculator 
    /// </summary>
    public sealed class FastDCTCalculator
    {
        #region private fields
        private readonly byte[,] _sourceMatrix;

        private static readonly ConcurrentDictionary<Tuple<int, int>, Complex> WFunctionCache =
            new ConcurrentDictionary<Tuple<int, int>, Complex>();
        #endregion

        #region ctor
        /// <summary>
        /// Construct a calcualtor from an image
        /// </summary>
        /// <param name="sourceImage"></param>
        public FastDCTCalculator(Image sourceImage)
        {
            if (sourceImage.Width != sourceImage.Height)
            {
                throw new ArgumentException("Image must be square for DCT transform");
            }
            _sourceMatrix = CopyImageToMatrix(sourceImage);
        }

        /// <summary>
        /// Construct a calculator from a given matrix
        /// </summary>
        /// <param name="sourceMatrix"></param>
        public FastDCTCalculator(byte[,] sourceMatrix)
        {
            _sourceMatrix = sourceMatrix;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Calculate the DCT
        /// </summary>
        /// <param name="sourceImage">The source image to find the DCT of</param>
        /// <returns>The DCT coefficients</returns>
        public static double[,] Calculate(Image sourceImage)
        {
            return (new FastDCTCalculator(sourceImage)).Calculate();
        }

        /// <summary>
        /// Calculate the DCT
        /// </summary>
        /// <param name="sourceMatrix">The source matrix to find the DCT of</param>
        /// <returns>The DCT coefficients</returns>
        public static double[,] Calculate(byte[,] sourceMatrix)
        {
            return (new FastDCTCalculator(sourceMatrix)).Calculate();
        }

        /// <summary>
        /// Calculate the DCT
        /// </summary>
        /// <returns>The DCT coefficients</returns>
        public double[,] Calculate()
        {
            int length = _sourceMatrix.GetLength(0);
            Complex[][] dft = CalculateDFT2D(CreateYSequence(_sourceMatrix));
            double[,] dctOutput = new double[length, length];
            for (int row = 0; row < length; row++)
            {
                for (int col = 0; col < _sourceMatrix.GetLength(1); col++)
                {
                    Complex dftAsComplex = WFunctionCached(2 * length, row) * WFunctionCached(2 * length, col) * dft[row][col];
                    dctOutput[row, col] = dftAsComplex.Real;
                }
            }

            return dctOutput;
        }
        #endregion

        #region private methods
        private static Complex[][] CalculateDFT2D(Complex[][] input)
        {
            // Compute 2D DFT
            Complex[][] output = new Complex[input.Length][];

            // Get rows first
            for (int row = 0; row < input.Length; row++)
            {
                output[row] = CalculateDFTSharp(input[row]);
            }

            // Get cols next
            for (int col = 0; col < input[0].Length; col++)
            {
                Complex[] columnDFT = CalculateDFTSharp(GetColumn(output, col).ToArray());
                SetColumn(columnDFT, output, col);
            }

            return NormalizeDFT(output);
        }

        private static byte[,] CopyImageToMatrix(Image image)
        {
            byte[,] sourceMatrix = new byte[image.Width, image.Height];
            using (var lockbitImage = new WritableLockBitImage(image))
            {
                for (int y = 0; y < lockbitImage.Height; y++)
                {
                    for (int x = 0; x < lockbitImage.Width; x++)
                    {
                        sourceMatrix[y, x] = lockbitImage.GetPixel(x, y).R;
                    }
                }
            }

            return sourceMatrix;
        }

        private static Complex[][] CreateYSequence(byte[,] sourceMatrix)
        {
            int sourceLength = sourceMatrix.GetLength(0);
            Complex[][] ySequence = new Complex[sourceLength * 2][];
            for (int y = 0; y < sourceLength * 2; y++)
            {
                ySequence[y] = new Complex[sourceLength * 2];
                for (int x = 0; x < sourceLength * 2; x++)
                {
                    if (y < sourceLength && x < sourceLength)
                    {
                        ySequence[y][x] = sourceMatrix[y, x];
                    }
                    else if (y >= sourceLength && x < sourceLength)
                    {
                        ySequence[y][x] = sourceMatrix[2 * sourceLength - 1 - y, x];
                    }
                    else if (y < sourceLength && x >= sourceLength)
                    {
                        ySequence[y][x] = sourceMatrix[y, 2 * sourceLength - 1 - x];
                    }
                    else if (y >= sourceLength && x >= sourceLength)
                    {
                        ySequence[y][x] = sourceMatrix[2 * sourceLength - 1 - y, 2 * sourceLength - 1 - x];
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown state");
                    }
                }
            }

            return ySequence;
        }

        private static Complex[] CalculateDFTSharp(Complex[] input)
        {
            var complexDoubleDFT = new SharpFFTPACK.ComplexDoubleFFT(input.Length);
            complexDoubleDFT.ft(input);
            return input;
        }

        private static Complex[][] NormalizeDFT(Complex[][] input)
        {
            int N = input.Length;
            int M = input[0].Length;

            Complex[][] result = new Complex[N][];
            for (int i = 0; i < N; i++)
            {
                result[i] = new Complex[M];
                for (int j = 0; j < M; j++)
                {
                    result[i][j] = (1 / Math.Sqrt(N * M)) * input[i][j];
                }
            }

            return result;
        }

        private static IEnumerable<Complex> GetColumn(Complex[][] input, int col)
        {
            for (int row = 0; row < input.Length; row++)
            {
                yield return input[row][col];
            }
        }

        private static void SetColumn(Complex[] column, Complex[][] dest, int colNumber)
        {
            for (int row = 0; row < dest.Length; row++)
            {
                dest[row][colNumber] = column[row];
            }
        }

        private static Complex WFunctionCached(int N, int expOfW)
        {
            return WFunctionCache.GetOrAdd(Tuple.Create(N, expOfW), t => WFunctionImpl(t.Item1, t.Item2));
        }

        private static Complex WFunctionImpl(int N, int expOfW)
        {
            return new Complex(Math.Cos((2 * Math.PI / N) * expOfW), -Math.Sin((2 * Math.PI / N) * expOfW));
        }
        #endregion
    }
}
