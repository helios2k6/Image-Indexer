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

using System;
using YAXLib;

namespace Core.Media
{
    /// <summary>
    /// Represents a single media track
    /// </summary>
    public sealed class Track : IEquatable<Track>
    {
        #region ctor
        public Track()
        {
            CompleteName = Type = Duration = Framerate = string.Empty;
            ID = -1;
        }
        #endregion

        #region public properties
        /// <summary>
        /// The type of media track
        /// </summary>
        [YAXAttributeForClass]
        [YAXSerializeAs("type")]
        [YAXErrorIfMissed(YAXExceptionTypes.Warning)]
        public string Type { get; set; }

        /// <summary>
        /// The duration of the media track
        /// </summary>
        [YAXSerializeAs("Duration")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public string Duration { get; set; }

        /// <summary>
        /// The track ID number
        /// </summary>
        [YAXSerializeAs("ID")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public int ID { get; set; }

        /// <summary>
        /// The complete name of the file, as specified in the General track
        /// </summary>
        [YAXSerializeAs("Complete_name")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public string CompleteName { get; set; }

        /// <summary>
        /// The frame rate of the video track
        /// </summary>
        [YAXSerializeAs("Frame_rate")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public string Framerate { get; set; }

        /// <summary>
        /// The width of the video track
        /// </summary>
        [YAXSerializeAs("Width")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public string Width { get; set; }

        /// <summary>
        /// The height of the video track
        /// </summary>
        [YAXSerializeAs("Height")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public string Height { get; set; }
        #endregion

        #region public methods
        public bool Equals(Track other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return Equals(Type, other.Type) &&
                Equals(Duration, other.Duration) &&
                Equals(ID, other.ID) &&
                Equals(CompleteName, other.CompleteName) &&
                Equals(Framerate, other.Framerate);
        }

        public override bool Equals(object other)
        {
            return Equals(other as Track);
        }

        public override int GetHashCode()
        {
            return GetHashCodeSafe(Type) ^
                GetHashCodeSafe(Duration) ^
                GetHashCodeSafe(ID) ^
                GetHashCodeSafe(CompleteName) ^
                GetHashCodeSafe(Framerate) ^
                GetHashCodeSafe(Width) ^
                GetHashCodeSafe(Height);
        }

        /// <summary>
        /// Get the Duration of the track as a TimeSpan object, isntead of a String, which is
        /// what it's serialized as
        /// </summary>
        /// <returns>
        /// A TimeSpan representing the duration of this track, or a 0 duration TimeSpan if the 
        /// Duration property is empty
        /// </returns>
        /// <remarks>
        /// The Duration property is normally serialized as a descriptive representation of the 
        /// time duration of a track (e.g. "1h 3mn"). This function is meant to parse and return
        /// that representation as a TimeSpan object for the sake of convenience
        /// </remarks>
        public TimeSpan GetDurationAsTimeSpan()
        {
            if (string.IsNullOrWhiteSpace(Duration))
            {
                return TimeSpan.FromSeconds(0);
            }

            int indexOfHourMarker = Duration.IndexOf("h");
            int indexOfMinuteMarker = Duration.IndexOf("min");
            int indexOfSecondMarker = Duration.IndexOf("s");

            int hours, minutes, seconds;
            hours = minutes = seconds = 0;
            // Chomp the hours off first
            if (indexOfHourMarker != -1)
            {
                string hoursAsString = Duration
                    .Substring(0, indexOfHourMarker)
                    .Trim();

                int.TryParse(hoursAsString, out hours);
            }

            // Chomp the minutes next
            if (indexOfMinuteMarker != -1)
            {
                // Check to see if hours is specified. If not, then we start at index
                // 0. Otherwise it starts at indexOfHourMarker + 1
                int startIndexForMinutes = indexOfHourMarker != -1
                    ? indexOfHourMarker + 1
                    : 0;

                string minutesAsString = Duration
                    .Substring(startIndexForMinutes, indexOfMinuteMarker - startIndexForMinutes)
                    .Trim();

                int.TryParse(minutesAsString, out minutes);
            }

            // Chomp seconds last
            if (indexOfSecondMarker != -1)
            {
                // Check to see if minutes is specified. If not, then we start at index
                // 0. Otherwise, it starts at indexOfMinutesMarker + 1
                int startIndexForSeconds = indexOfMinuteMarker != -1
                    ? indexOfMinuteMarker + 3
                    : 0;

                string secondsAsString = Duration
                    .Substring(startIndexForSeconds, indexOfSecondMarker - startIndexForSeconds)
                    .Trim();

                int.TryParse(secondsAsString, out seconds);
            }

            return new TimeSpan(hours, minutes, seconds);
        }
        #endregion

        #region private methods
        private static int GetHashCodeSafe(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj.GetHashCode();
        }

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
