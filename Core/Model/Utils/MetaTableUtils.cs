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

using Core.Model.Serialization;
using Core.Model.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace Core.Model.Utils
{
    /// <summary>
    /// Utils for the VideoFingerPrintDAtabaseMetaTable
    /// </summary>
    public static class MetaTableUtils
    {
        /// <summary>
        /// Enumerate all databases in the metatable
        /// </summary>
        /// <param name="metatable">The metatable</param>
        /// <returns>An enumeration of all of the databases</returns>
        public static IEnumerable<VideoFingerPrintDatabaseWrapper> EnumerateDatabases(VideoFingerPrintDatabaseMetaTableWrapper metatable)
        {
            return from entry in metatable.DatabaseMetaTableEntries
                   select VideoFingerPrintDatabaseLoader.Load(entry.FileName);
        }

        /// <summary>
        /// Enumerate all of the database fingerprints given a metatable
        /// </summary>
        /// <param name="metatable">The metatable</param>
        /// <returns>An enumeration of all of the fingerprints</returns>
        public static IEnumerable<VideoFingerPrintWrapper> EnumerateVideoFingerPrints(VideoFingerPrintDatabaseMetaTableWrapper metatable)
        {
            return from database in EnumerateDatabases(metatable)
                   from fingerprint in database.VideoFingerPrints
                   select fingerprint;
        }

        /// <summary>
        /// Enumerate frame fingerprints
        /// </summary>
        /// <param name="metatable">The metatable</param>
        /// <returns>An enumeration of all of the frame fingerprints</returns>
        public static IEnumerable<FrameFingerPrintWrapper> EnumerateFrameFingerPrints(VideoFingerPrintDatabaseMetaTableWrapper metatable)
        {
            return from videoFingerPrint in EnumerateVideoFingerPrints(metatable)
                   from frame in videoFingerPrint.FingerPrints
                   select frame;
        }
    }
}