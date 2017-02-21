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

using Core.Numerics;
using Functional.Maybe;
using System;
using System.Linq;
using YAXLib;

namespace Core.Media
{
    /// <summary>
    /// Represents the media information of a media file
    /// </summary>
    [Serializable]
    [YAXSerializeAs("Mediainfo")]
    public sealed class MediaInfo : IEquatable<MediaInfo>
    {
        #region private fields
        private static readonly string GENERAL_TRACK = "General";
        private static readonly string VIDEO_TRACK = "Video";

        private readonly Lazy<Maybe<Track>> _generalTrack;
        private readonly Lazy<Maybe<Track>> _videoTrack;
        private readonly Lazy<TimeSpan> _duration;
        private readonly Lazy<Ratio> _framerate;
        private readonly Lazy<int> _width;
        private readonly Lazy<int> _height;
        #endregion

        #region ctor
        public MediaInfo()
        {
            _generalTrack = new Lazy<Maybe<Track>>(CalculateGeneralTrack);
            _videoTrack = new Lazy<Maybe<Track>>(CalculateVideoTrack);
            _duration = new Lazy<TimeSpan>(CalculateDuration);
            _framerate = new Lazy<Ratio>(CalculateFramerate);
            _width = new Lazy<int>(CalculateWidth);
            _height = new Lazy<int>(CalculateHeight);
        }
        #endregion

        #region public properties
        /// <summary>
        /// The path to the media file
        /// </summary>
        [YAXSerializeAs("File")]
        public FileXMLNode File { get; set; }
        #endregion

        #region public methods
        /// <summary>
        /// Get the name of the file that this MediaInfo describes
        /// </summary>
        /// <returns>The file name or string.Empty if it cannot be determined</returns>
        public string GetFileName()
        {
            return _generalTrack.Value.SelectOrElse(t => t.CompleteName, () => string.Empty);
        }

        /// <summary>
        /// Get the duration of this media file, if possible
        /// </summary>
        /// <returns>
        /// Returns the duration of this media file as a TimeSpan or a TimeSpan of 0 if the duration
        /// cannot be determined
        /// </returns>
        public TimeSpan GetDuration()
        {
            return _duration.Value;
        }

        /// <summary>
        /// Get the framerate of the video track
        /// </summary>
        public Ratio GetFramerate()
        {
            return _framerate.Value;
        }

        /// <summary>
        /// Get the width of the video
        /// </summary>
        /// <returns></returns>
        public int GetWidth()
        {
            return _width.Value;
        }

        /// <summary>
        /// Get the height of the video
        /// </summary>
        /// <returns></returns>
        public int GetHeight()
        {
            return _height.Value;
        }

        public bool Equals(MediaInfo other)
        {
            if (EqualsPreamble(other) == false)
            {
                return false;
            }

            return Equals(File, other.File);
        }

        public override bool Equals(object other)
        {
            return Equals(other as MediaInfo);
        }

        public override int GetHashCode()
        {
            return File.GetHashCode();
        }
        #endregion

        #region private methods
        private TimeSpan CalculateDuration()
        {
            return _generalTrack.Value.SelectOrElse(t => t.GetDurationAsTimeSpan(), () => TimeSpan.FromSeconds(0));
        }

        private Maybe<Track> CalculateGeneralTrack()
        {
            return (from track in File.Tracks
                    where string.Equals(track.Type, GENERAL_TRACK, StringComparison.OrdinalIgnoreCase)
                    select track).FirstOrDefault().ToMaybe();
        }

        private Maybe<Track> CalculateVideoTrack()
        {
            return (from track in File.Tracks
                    where string.Equals(track.Type, VIDEO_TRACK, StringComparison.OrdinalIgnoreCase)
                    select track).FirstOrDefault().ToMaybe();
        }

        private int CalculateWidth()
        {
            Maybe<string> rawTrackTextMaybe = from track in _videoTrack.Value
                                              select track.Width;

            Maybe<string> width = from text in rawTrackTextMaybe
                                  let indexOfPixels = text.IndexOf("pixels")
                                  let substringToPixels = text.Substring(0, indexOfPixels)
                                  let implodedString = substringToPixels.Replace(" ", string.Empty)
                                  select implodedString;

            return int.Parse(width.OrElse("0"));
        }

        private int CalculateHeight()
        {
            Maybe<string> rawTrackTextMaybe = from track in _videoTrack.Value
                                              select track.Height;

            Maybe<string> height = from text in rawTrackTextMaybe
                                   let indexOfPixels = text.IndexOf("pixels")
                                   let substringToPixels = text.Substring(0, indexOfPixels)
                                   let implodedString = substringToPixels.Replace(" ", string.Empty)
                                   select implodedString;

            return int.Parse(height.OrElse("0"));
        }

        private Ratio CalculateFramerate()
        {
            Maybe<string> rawTrackTextMaybe = from track in _videoTrack.Value
                                              select track.Framerate;

            Maybe<Ratio> fpsFromParenthesis = from framerateText in rawTrackTextMaybe
                                              let startParenths = framerateText.IndexOf("(")
                                              let endParenths = framerateText.IndexOf(")")
                                              where startParenths != -1 && endParenths != -1
                                              let fpsSubstring = framerateText.Substring(startParenths + 1, endParenths - startParenths - 1)
                                              let splitOnSlash = fpsSubstring.Split('/')
                                              where splitOnSlash.Length == 2
                                              let numerator = NumericUtils.TryParseInt(splitOnSlash[0])
                                              let denominator = NumericUtils.TryParseInt(splitOnSlash[1])
                                              where numerator != null && denominator != null
                                              select new Ratio(numerator.Value, denominator.Value);

            Maybe<Ratio> fpsFromDirectParse = from framerateText in rawTrackTextMaybe
                                              let indexOfFpsMarker = framerateText.IndexOf("fps", StringComparison.OrdinalIgnoreCase)
                                              where indexOfFpsMarker != -1
                                              let fpsAsDecimal = framerateText.Substring(0, indexOfFpsMarker - 2)
                                              let fpsAsDouble = NumericUtils.TryParseDouble(fpsAsDecimal)
                                              where fpsAsDouble != null
                                              select NumericUtils.ConvertDoubleToFPS(fpsAsDouble.Value);

            return fpsFromParenthesis.Or(fpsFromDirectParse).OrElse(new Ratio());
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