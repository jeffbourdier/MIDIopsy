/* MidiSetTempoEvent.cs - Implementation of MidiSetTempoEvent class,
 * which corresponds to a Set Tempo meta-event in the MIDI file spec.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


namespace JeffBourdier
{
    /// <summary>Corresponds to a Set Tempo meta-event in the MIDI file spec.</summary>
    public sealed class MidiSetTempoEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiSetTempoEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiSetTempoEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 0x51, bytes, 6, index)
        {
            this.ValidateDataLength(3, Properties.Resources.SetTempo);
            this._Tempo = MidiData.ReadNumber(bytes, 3, index + this.DataLengthSize);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.SetTempo, this.DataLength);
            this.DataComment = string.Format("{0} {1} ({2} {3})",
                this.Tempo, Properties.Resources.MicrosecondsPerQuarterNote,
                (60000000 / this.Tempo), Properties.Resources.BeatsPerMinute);
            this.SetComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _Tempo;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Number of microseconds per MIDI quarter-note.</summary>
        public uint Tempo
        {
            get { return this._Tempo; }

            set
            {
                MidiData.ValidateNumber(value, 0, 0xFFFFFF, Properties.Resources.SetTempo);
                MidiData.WriteNumber(value, 3, this.Bytes, this.DataIndex);
                this._Tempo = value;
                this.DataComment = value.ToString();
                this.SetComment();
            }
        }

        #endregion
    }
}
