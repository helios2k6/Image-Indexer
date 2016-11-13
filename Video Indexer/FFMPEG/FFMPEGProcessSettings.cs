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

namespace VideoIndexer
{
    /// <summary>
    /// Specifies the settings to use for running the FFMPEG Process
    /// </summary>
    internal sealed class FFMPEGProcessSettings : IEquatable<FFMPEGProcessSettings>
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
        /// The time to seek to before outputting images
        /// </summary>
        public TimeSpan StartTime { get; }

        /// <summary>
        /// The number of frames to output
        /// </summary>
        public int FramesToOutput { get; }

        /// <summary>
        /// The frame rate to playback the video
        /// </summary>
        public Ratio Framerate { get; }

        /// <summary>
        /// The output format to use for the FFMPEG Process
        /// </summary>
        public FFMPEGOutputFormat OutputFormat { get; }
        #endregion

        #region ctor
        /// <summary>
        /// Construct a new FFMPEGProcessSettings object to configure how to run the FFMPEG
        /// process
        /// </summary>
        /// <param name="targetMediaFile">The media file to decode</param>
        /// <param name="outputDirectory">The path to the folder where the output images will be placed</param>
        /// <param name="startTime">The time to seek to before outputting images</param>
        /// <param name="framesToOutput">The number of frames to output</param>
        /// <param name="framerate">The playback framerate</param>
        public FFMPEGProcessSettings(
            string targetMediaFile,
            string outputDirectory,
            TimeSpan startTime,
            int framesToOutput,
            Ratio framerate,
            FFMPEGOutputFormat outputFormat
        )
        {
            TargetMediaFile = targetMediaFile;
            OutputDirectory = outputDirectory;
            StartTime = startTime;
            FramesToOutput = framesToOutput;
            Framerate = framerate;
            OutputFormat = outputFormat;
        }

        /// <summary>
        /// Construct a new FFMPEGProcessSettings object to configure how to run the FFMPEG
        /// process, but using the settings for harvesting images for comparison
        /// </summary>
        /// <param name="targetMediaFile">The media file to decode</param>
        /// <param name="startTime">The time to seek to before outputting images</param>
        public FFMPEGProcessSettings(
            string targetMediaFile,
            TimeSpan startTime
        ) : this(targetMediaFile, string.Empty, startTime, 8, new Ratio(4, 1), FFMPEGOutputFormat.PNG)
        {
        }
        #endregion

        #region public methods
        public override bool Equals(object other)
        {
            return Equals(other as FFMPEGProcessSettings);
        }

        public override int GetHashCode()
        {
            return TargetMediaFile.GetHashCode() ^
                OutputDirectory.GetHashCode() ^
                StartTime.GetHashCode() ^
                FramesToOutput.GetHashCode() ^
                Framerate.GetHashCode() ^
                OutputFormat.GetHashCode();
        }

        public bool Equals(FFMPEGProcessSettings other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return Equals(TargetMediaFile, other.TargetMediaFile) &&
                Equals(OutputDirectory, other.OutputDirectory) &&
                Equals(StartTime, other.StartTime) &&
                Equals(FramesToOutput, other.FramesToOutput) &&
                Equals(Framerate, other.Framerate) &&
                Equals(OutputFormat, other.OutputFormat);
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
