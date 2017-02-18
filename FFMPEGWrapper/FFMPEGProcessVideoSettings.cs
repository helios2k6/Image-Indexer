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

namespace FFMPEGWrapper
{
    /// <summary>
    /// Specifies the settings to use for running the FFMPEG Process
    /// </summary>
    public sealed class FFMPEGProcessVideoSettings : IEquatable<FFMPEGProcessVideoSettings>
    {
        #region public properties
        /// <summary>
        /// The media file to decode using FFMPEG
        /// </summary>
        public string TargetMediaFile { get; }

        /// <summary>
        /// The path to the folder where the output images will be placed
        /// </summary>
        public string OutputDirectory { get; }

        /// <summary>
        /// The frame rate numerator
        /// </summary>
        public int FrameRateNumerator { get; }

        /// <summary>
        /// The frame rate denominator
        /// </summary>
        public int FrameRateDenominator { get; }
        #endregion

        #region ctor
        /// <summary>
        /// Construct a new FFMPEGProcessSettings object to configure how to run the FFMPEG
        /// process
        /// </summary>
        /// <param name="targetMediaFile">The media file to decode</param>
        /// <param name="frameRateDenominator">The denominator of the frame rate</param>
        /// <param name="frameRateNumerator">The numerator of the frame rate</param>
        public FFMPEGProcessVideoSettings(
            string targetMediaFile,
            int frameRateNumerator,
            int frameRateDenominator
        )
        {
            TargetMediaFile = targetMediaFile;
            FrameRateNumerator = frameRateNumerator;
            FrameRateDenominator = frameRateDenominator;
        }
        #endregion

        #region public methods
        public override bool Equals(object other)
        {
            return Equals(other as FFMPEGProcessVideoSettings);
        }

        public override int GetHashCode()
        {
            return TargetMediaFile.GetHashCode() ^
                FrameRateNumerator.GetHashCode() ^
                FrameRateDenominator.GetHashCode();
        }

        public bool Equals(FFMPEGProcessVideoSettings other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return Equals(TargetMediaFile, other.TargetMediaFile) &&
                Equals(OutputDirectory, other.OutputDirectory) &&
                Equals(FrameRateNumerator, other.FrameRateNumerator) &&
                Equals(FrameRateDenominator, other.FrameRateDenominator);
        }
        #endregion

        #region private methods
        private bool EqualsPreamble(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return true;
        }
        #endregion
    }
}
