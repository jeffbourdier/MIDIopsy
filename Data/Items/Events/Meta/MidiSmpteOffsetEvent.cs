/* MidiSmpteOffsetEvent.cs - Implementation of MidiSmpteOffsetEvent class,
 * which corresponds to a SMPTE Offset meta-event in the MIDI file spec.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


namespace JeffBourdier
{
    /// <summary>Corresponds to a SMPTE Offset meta-event in the MIDI file spec.</summary>
    public sealed class MidiSmpteOffsetEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiSmpteOffsetEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiSmpteOffsetEvent(MidiTrackChunk owner, int deltaTime, byte[] bytes, int index)
            : base(owner, deltaTime, 0x54, bytes, 8, index)
        {
            this.ValidateDataLength(5, Properties.Resources.SmpteOffset);
            this._Hour = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize);
            this._Minute = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize + 1);
            this._Second = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize + 2);
            this._Frame = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize + 3);
            this._Centiframe = MidiData.ReadNumber(bytes, 1, index + this.DataLengthSize + 4);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.SmpteOffset, this.DataLength);
            this.SetDataComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _Hour;
        private uint _Minute;
        private uint _Second;
        private uint _Frame;
        private uint _Centiframe;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>The hour component of the SMPTE time.</summary>
        public uint Hour
        {
            get { return this._Hour; }

            set
            {
                MidiData.ValidateNumber(value, 0, 23, Properties.Resources.Hour);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex);
                this._Hour = value;
                this.SetDataComment();
            }
        }

        /// <summary>The minute component of the SMPTE time.</summary>
        public uint Minute
        {
            get { return this._Minute; }

            set
            {
                MidiData.ValidateNumber(value, 0, 59, Properties.Resources.Minute);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex + 1);
                this._Minute = value;
                this.SetDataComment();
            }
        }

        /// <summary>The second component of the SMPTE time.</summary>
        public uint Second
        {
            get { return this._Second; }

            set
            {
                MidiData.ValidateNumber(value, 0, 59, Properties.Resources.Second);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex + 2);
                this._Second = value;
                this.SetDataComment();
            }
        }

        /// <summary>The frame component of the SMPTE time.</summary>
        public uint Frame
        {
            get { return this._Frame; }

            set
            {
                MidiData.ValidateNumber(value, 0, 29, Properties.Resources.Frame);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex + 3);
                this._Frame = value;
                this.SetDataComment();
            }
        }

        /// <summary>The fractional frame component of the SMPTE time, in hundredths of a frame.</summary>
        public uint Centiframe
        {
            get { return this._Centiframe; }

            set
            {
                MidiData.ValidateNumber(value, 0, 99, Properties.Resources.Centiframe);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DataIndex + 4);
                this._Centiframe = value;
                this.SetDataComment();
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private void SetDataComment()
        {
            this.DataComment = string.Format("{0}:{1}:{2}:{3}.{4}", this.Hour.ToString("00"), this.Minute.ToString("00"),
                this.Second.ToString("00"), this.Frame.ToString("00"), this.Centiframe.ToString("00"));
            this.SetComment();
        }

        #endregion
    }
}
