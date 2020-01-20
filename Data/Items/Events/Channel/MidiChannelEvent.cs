/* MidiChannelEvent.cs - Implementation of MidiChannelEvent class, which corresponds
 * to a MIDI event (i.e., a MIDI channel message) in the MIDI file spec.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* ApplicationException */
using System;


namespace JeffBourdier
{
    /// <summary>Corresponds to a MIDI event (i.e., a MIDI channel message) in the MIDI file spec.</summary>
    public abstract class MidiChannelEvent : MidiEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiChannelEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="messageType">
        /// Identifies the type of channel message (corresponds to the high nibble of the event's status byte).
        /// </param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which this event is transmitted.</param>
        /// <param name="length">
        /// Number of bytes in the event (not counting delta-time), used to instantiate the byte array.
        /// This number should be 2 or 3 (i.e., 1 status byte plus 1 or 2 data bytes, depending on the message type).
        /// </param>
        /// <param name="description">A short description of the channel message (for display purposes).</param>
        /// <remarks>To create an event with running status, use the other constructor.</remarks>
        protected MidiChannelEvent(MidiTrackChunk owner, int deltaTime,
            uint messageType, uint channel, int length, string description)
            : base(owner, deltaTime, length)
        {
            MidiData.ValidateNumber(messageType, 0x8, 0xE, Properties.Resources.MessageType);
            this._RunningStatus = false;
            this.MessageTypeComment = description;
            this._MessageType = messageType;
            this.Channel = channel;
            this._DataOffset = 1;
        }

        /// <summary>Initializes a new instance of the MidiChannelEvent class using running status.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="length">
        /// Number of bytes in the event (not counting delta-time), used to instantiate the byte array.
        /// This number should be 1 or 2 (data bytes only; no status byte with running status).
        /// </param>
        protected MidiChannelEvent(MidiTrackChunk owner, int deltaTime, int length)
            : base(owner, deltaTime, length)
        {
            this._RunningStatus = true;
            this._MessageType = uint.MaxValue;
            this._Channel = uint.MaxValue;
            this.TypeComment = new string(' ', 22);
            this.SetComment();
            this._DataOffset = 0;
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private bool _RunningStatus;
        private string MessageTypeComment;
        private uint _MessageType;
        private uint _Channel;
        private int _DataOffset;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Representation of the byte array in hexadecimal format.</summary>
        public override string Hex
        {
            get
            {
                string s = base.Hex;
                return this.RunningStatus ? (s.Substring(0, 12) + "   " + s.Substring(12)) : s;
            }
        }

        /// <summary>Indicates whether or not this event was created with running status.</summary>
        public bool RunningStatus { get { return this._RunningStatus; } }

        /// <summary>Identifies the type of channel message, or uint.MaxValue to indicate running status.</summary>
        /// <remarks>This corresponds to the high nibble of the event's status byte.</remarks>
        public uint MessageType { get { return this._MessageType; } }

        /// <summary>
        /// One of the sixteen logical MIDI channels on which this event
        /// is transmitted, or uint.MaxValue to indicate running status.
        /// </summary>
        /// <remarks>This corresponds to the low nibble of the event's status byte.</remarks>
        public uint Channel
        {
            get { return this._Channel; }

            set
            {
                /* If this event uses running status, its channel cannot be changed arbitrarily. */
                if (this.RunningStatus) throw new ApplicationException(Properties.Resources.ChannelNotChanged);

                MidiData.ValidateNumber(value, 0, 0xF, Properties.Resources.Channel);
                MidiData.WriteNumber((this.MessageType << 4) | value, 1, this.Bytes, this.DeltaTimeSize);
                this._Channel = value;
                this.TypeComment = string.Format("{0,-17} | {1,2}", this.MessageTypeComment, this.Channel);
                this.SetComment();
            }
        }

        #endregion

        #region Protected Properties

        public int DataOffset { get { return this._DataOffset; } }

        public bool IsPercussion
        { get { return this.Channel == 9 || (this.RunningStatus && this.Track.GetChannel(this.CumulativeTime) == 9); } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Protected Methods

        /// <summary>Converts a numeric value to a string representation of its assigned note(s).</summary>
        /// <param name="n">Numeric value assigned to note (middle C has a reference value of 60).</param>
        /// <param name="format">True to format the string using a predefind field width; otherwise, false.</param>
        /// <returns>String representation of note(s) to which numeric value is assigned.</returns>
        protected string NumberToNote(uint n, bool format)
        {
            string s = null;

            /* If appropriate, notate as percussion. */
            if (this.IsPercussion)
            {
                switch (n)
                {
                    case 35: s = Properties.Resources.Percussion35; break;
                    case 36: s = Properties.Resources.Percussion36; break;
                    case 37: s = Properties.Resources.Percussion37; break;
                    case 38: s = Properties.Resources.Percussion38; break;
                    case 39: s = Properties.Resources.Percussion39; break;
                    case 40: s = Properties.Resources.Percussion40; break;
                    case 41: s = Properties.Resources.Percussion41; break;
                    case 42: s = Properties.Resources.Percussion42; break;
                    case 43: s = Properties.Resources.Percussion43; break;
                    case 44: s = Properties.Resources.Percussion44; break;
                    case 45: s = Properties.Resources.Percussion45; break;
                    case 46: s = Properties.Resources.Percussion46; break;
                    case 47: s = Properties.Resources.Percussion47; break;
                    case 48: s = Properties.Resources.Percussion48; break;
                    case 49: s = Properties.Resources.Percussion49; break;
                    case 50: s = Properties.Resources.Percussion50; break;
                    case 51: s = Properties.Resources.Percussion51; break;
                    case 52: s = Properties.Resources.Percussion52; break;
                    case 53: s = Properties.Resources.Percussion53; break;
                    case 54: s = Properties.Resources.Percussion54; break;
                    case 55: s = Properties.Resources.Percussion55; break;
                    case 56: s = Properties.Resources.Percussion56; break;
                    case 57: s = Properties.Resources.Percussion57; break;
                    case 58: s = Properties.Resources.Percussion58; break;
                    case 59: s = Properties.Resources.Percussion59; break;
                    case 60: s = Properties.Resources.Percussion60; break;
                    case 61: s = Properties.Resources.Percussion61; break;
                    case 62: s = Properties.Resources.Percussion62; break;
                    case 63: s = Properties.Resources.Percussion63; break;
                    case 64: s = Properties.Resources.Percussion64; break;
                    case 65: s = Properties.Resources.Percussion65; break;
                    case 66: s = Properties.Resources.Percussion66; break;
                    case 67: s = Properties.Resources.Percussion67; break;
                    case 68: s = Properties.Resources.Percussion68; break;
                    case 69: s = Properties.Resources.Percussion69; break;
                    case 70: s = Properties.Resources.Percussion70; break;
                    case 71: s = Properties.Resources.Percussion71; break;
                    case 72: s = Properties.Resources.Percussion72; break;
                    case 73: s = Properties.Resources.Percussion73; break;
                    case 74: s = Properties.Resources.Percussion74; break;
                    case 75: s = Properties.Resources.Percussion75; break;
                    case 76: s = Properties.Resources.Percussion76; break;
                    case 77: s = Properties.Resources.Percussion77; break;
                    case 78: s = Properties.Resources.Percussion78; break;
                    case 79: s = Properties.Resources.Percussion79; break;
                    case 80: s = Properties.Resources.Percussion80; break;
                    case 81: s = Properties.Resources.Percussion81; break;
                }
                if (!string.IsNullOrEmpty(s)) return format ? string.Format("{0,-18}", s) : s;
            }

            /* Otherwise, notate as pitch. */
            uint u = (n / 12) - 1;

            /* If there is no key signature, assume C major. */
            MidiKeySignature k = this.Track.GetKeySignature(this.CumulativeTime);
            if (k == MidiKeySignature.NA) k = MidiKeySignature.CMajor;

            /* Convert the numeric value based on key signature (see MidiKeySignature notation chart). */
            switch (n % 12)
            {
                case 0:
                    if (k > MidiKeySignature.EMajor && k < MidiKeySignature.CFlatMajor) { s = "B#"; --u; }
                    else s = "C"; break;
                case 1: s = (k > MidiKeySignature.FMajor || k < MidiKeySignature.CFlatMajor) ? "C#" : "Db"; break;
                case 2: s = "D"; break;
                case 3: s = (k > MidiKeySignature.GMajor && k < MidiKeySignature.CFlatMajor) ? "D#" : "Eb"; break;
                case 4: s = (k == MidiKeySignature.CFlatMajor || k == MidiKeySignature.AFlatMinor) ? "Fb" : "E"; break;
                case 5: s = (k > MidiKeySignature.AMajor && k < MidiKeySignature.CFlatMajor) ? "E#" : "F"; break;
                case 6: s = (k > MidiKeySignature.BFlatMinor || k < MidiKeySignature.CFlatMajor) ? "F#" : "Gb"; break;
                case 7: s = "G"; break;
                case 8: s = (k > MidiKeySignature.CMajor && k < MidiKeySignature.CFlatMajor) ? "G#" : "Ab"; break;
                case 9: s = "A"; break;
                case 10: s = (k > MidiKeySignature.DMajor && k < MidiKeySignature.CFlatMajor) ? "A#" : "Bb"; break;
                case 11:
                    if (k >= MidiKeySignature.CFlatMajor && k < MidiKeySignature.DFlatMajor) { s = "Cb"; ++u; }
                    else s = "B"; break;
                default: return string.Empty;
            }
            return (format ? string.Format("{0,-2}", s) : s) + " " + ((u < 0) ? "-" : ((u > 9) ? "+" : u.ToString()));
        }

        #endregion
    }
}
