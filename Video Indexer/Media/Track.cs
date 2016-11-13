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
using YAXLib;

namespace VideoIndexer.Media
{
    internal sealed class Track : IEquatable<Track>
    {
        #region ctor
        public Track()
        {
            CompleteName = Type = Duration = Framerate = string.Empty;
            ID = -1;
        }
        #endregion

        #region public properties
        [YAXAttributeForClass]
        [YAXSerializeAs("type")]
        [YAXErrorIfMissed(YAXExceptionTypes.Warning)]
        public string Type { get; set; }

        [YAXSerializeAs("Duration")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public string Duration { get; set; }

        [YAXSerializeAs("ID")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public int ID { get; set; }

        [YAXSerializeAs("Complete_name")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public string CompleteName { get; set; }

        [YAXSerializeAs("Frame_rate")]
        [YAXErrorIfMissedAttribute(YAXExceptionTypes.Ignore)]
        public string Framerate { get; set; }
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
            return Type.GetHashCode() ^
                Duration.GetHashCode() ^
                ID.GetHashCode() ^
                CompleteName.GetHashCode() ^
                Framerate.GetHashCode();
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
            int indexOfMinuteMarker = Duration.IndexOf("mn");
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
                    ? indexOfMinuteMarker + 2
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
