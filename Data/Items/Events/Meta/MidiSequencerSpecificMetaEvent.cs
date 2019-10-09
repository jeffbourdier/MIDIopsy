/* MidiSequencerSpecificMetaEvent.cs - Implementation of MidiSequencerSpecificMetaEvent
 * class, which corresponds to a Sequencer-Specific meta-event in the MIDI file spec.
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
    /// <summary>Corresponds to a Sequencer-Specific meta-event in the MIDI file spec.</summary>
    public sealed class MidiSequencerSpecificMetaEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiSequencerSpecificMetaEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiSequencerSpecificMetaEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 0x7F, bytes, 3, index)
        {
            this._Data = MidiData.ReadText(bytes, this.DataLength, index + this.DataLengthSize);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.SequencerSpecificMetaEvent, this.DataLength);
            this.DataComment = string.Format("\"{0}\"", this.Data);
            this.SetComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private string _Data;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        public string Data
        {
            get { return this._Data; }

            set
            {
                this.StoreText(value, this.DataIndex, this.Data.Length);
                this._Data = value;
                this.DataComment = string.Format("\"{0}\"", value);
                this.SetComment();
            }
        }

        #endregion
    }
}
