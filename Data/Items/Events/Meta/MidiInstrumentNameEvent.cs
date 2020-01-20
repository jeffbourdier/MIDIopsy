/* MidiInstrumentNameEvent.cs - Implementation of MidiInstrumentNameEvent class,
 * which corresponds to a Instrument Name meta-event in the MIDI file spec.
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
    /// <summary>Corresponds to a Instrument Name meta-event in the MIDI file spec.</summary>
    public sealed class MidiInstrumentNameEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiInstrumentNameEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiInstrumentNameEvent(MidiTrackChunk owner, int deltaTime, byte[] bytes, int index)
            : base(owner, deltaTime, 0x04, bytes, 3, index)
        {
            this._InstrumentName = MidiData.ReadText(bytes, this.DataLength, index + this.DataLengthSize);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.InstrumentName, this.DataLength);
            this.DataComment = string.Format("\"{0}\"", this.InstrumentName);
            this.SetComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private string _InstrumentName;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>A description of the type of instrumentation to be used.</summary>
        public string InstrumentName
        {
            get { return this._InstrumentName; }

            set
            {
                this.StoreText(value, this.DataIndex, this.InstrumentName.Length);
                this._InstrumentName = value;
                this.DataComment = string.Format("\"{0}\"", value);
                this.SetComment();
            }
        }

        #endregion
    }
}
