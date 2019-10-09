/* MidiCuePointEvent.cs - Implementation of MidiCuePointEvent class,
 * which corresponds to a Cue Point meta-event in the MIDI file spec.
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
    /// <summary>Corresponds to a Cue Point meta-event in the MIDI file spec.</summary>
    public sealed class MidiCuePointEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiCuePointEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiCuePointEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 0x07, bytes, 3, index)
        {
            this._CuePoint = MidiData.ReadText(bytes, this.DataLength, index + this.DataLengthSize);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.CuePoint, this.DataLength);
            this.DataComment = string.Format("\"{0}\"", this.CuePoint);
            this.SetComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private string _CuePoint;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>
        /// A description of something happening on a film or video screen or stage at a certain point in the musical score.
        /// </summary>
        public string CuePoint
        {
            get { return this._CuePoint; }

            set
            {
                this.StoreText(value, this.DataIndex, this.CuePoint.Length);
                this._CuePoint = value;
                this.DataComment = string.Format("\"{0}\"", value);
                this.SetComment();
            }
        }

        #endregion
    }
}
