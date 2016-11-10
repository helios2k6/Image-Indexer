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
using System.Collections.Generic;
using System.Linq;

namespace ImageIndexer
{
    /// <summary>
    /// Represents the Image Finger Print database
    /// </summary>
    public sealed class ImageFingerPrintDatabaseWrapper
    {
        #region private fields
        private readonly IList<ImageFingerPrintWrapper> _fingerPrints;
        private readonly IDictionary<string, ImageFingerPrintWrapper> _fileNameToFingerPrintMap;
        #endregion

        #region ctor
        /// <summary>
        /// Constructs a new ImageFingerPrintDatabaseWrapper
        /// </summary>
        /// <param name="fingerPrints">The fingerprints</param>
        public ImageFingerPrintDatabaseWrapper(ImageFingerPrintWrapper[] fingerPrints)
        {
            _fingerPrints = new List<ImageFingerPrintWrapper>(fingerPrints);
            _fileNameToFingerPrintMap = fingerPrints.ToDictionary(f => f.FilePath);
        }
        #endregion

        #region public methods
        /// <summary>
        /// Attempt to get the fingerprint for a specific file
        /// </summary>
        /// <param name="fileName">The path to the file</param>
        /// <returns>A fingerprint or none</returns>
        public Maybe<ImageFingerPrintWrapper> TryGetFingerPrint(string fileName)
        {
            ImageFingerPrintWrapper output;
            if (_fileNameToFingerPrintMap.TryGetValue(fileName, out output))
            {
                return output.ToMaybe();
            }

            return Maybe<ImageFingerPrintWrapper>.Nothing;
        }

        /// <summary>
        /// Adds a new fingerprint to the database
        /// </summary>
        /// <param name="fingerPrint">The fingerprint you want to add to the database</param>
        public void AddFingerPrint(ImageFingerPrintWrapper fingerPrint)
        {
            _fingerPrints.Add(fingerPrint);
            _fileNameToFingerPrintMap[fingerPrint.FilePath] = fingerPrint;
        }
        #endregion
    }
}
