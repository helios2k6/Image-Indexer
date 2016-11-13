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

using System.Collections.Generic;
using Functional.Maybe;
using System;

namespace VideoIndexer.Y4M
{
    /// <summary>
    /// A parser used to parse the file-level header parameters
    /// </summary>
    public sealed class FileHeaderParser : HeaderParser
    {
        #region private static fields
        private static Lazy<FileHeaderParser> SingletonInstance = new Lazy<FileHeaderParser>(() => new FileHeaderParser());
        #endregion

        #region ctor
        private FileHeaderParser()
        {
        }
        #endregion

        #region public static properties
        /// <summary>
        /// Get the singleton instance of this class
        /// </summary>
        public static FileHeaderParser Instance
        {
            get { return SingletonInstance.Value; }
        }
        #endregion

        #region protected methods
        protected override string HeaderMagicTag
        {
            get { return "YUV4MPEG2"; }
        }

        protected override Maybe<Header> TryConstructHeader(
            Maybe<int> width,
            Maybe<int> height,
            Maybe<Ratio> framerate,
            Maybe<Ratio> pixelAspectRatio,
            Maybe<Interlacing> interlacing,
            Maybe<ColorSpace> colorspace,
            IEnumerable<string> comments
        )
        {
            if (width.IsNothing() || height.IsNothing() || framerate.IsNothing())
            {
                return Maybe<Header>.Nothing;
            }

            return (new FileHeader(
                width.Value,
                height.Value,
                framerate.Value,
                pixelAspectRatio,
                colorspace,
                interlacing,
                comments
            ) as Header).ToMaybe();
        }
        #endregion
    }
}
