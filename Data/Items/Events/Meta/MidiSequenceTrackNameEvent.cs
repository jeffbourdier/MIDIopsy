/* MidiSequenceTrackNameEvent.cs - Implementation of MidiSequenceTrackNameEvent class,
 * which corresponds to a Sequence/Track Name meta-event in the MIDI file spec.
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
    /// <summary>Corresponds to a Sequence/Track Name meta-event in the MIDI file spec.</summary>
    public sealed class MidiSequenceTrackNameEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiSequenceTrackNameEvent class.</summary>
        /// <param name="owner">The track (MTrk) chunk to which this event belongs.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiSequenceTrackNameEvent(MidiTrackChunk owner, int deltaTime, byte[] bytes, int index)
            : base(owner, deltaTime, 0x03, bytes, 3, index)
        {
            this._SequenceTrackName = MidiData.ReadText(bytes, this.DataLength, index + this.DataLengthSize);
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.SequenceTrackName, this.DataLength);
            this.DataComment = string.Format("\"{0}\"", this.SequenceTrackName);
            this.SetComment();
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private string _SequenceTrackName;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>
        /// If in a format 0 track, or the first track in a format 1 file,
        /// the name of the sequence.  Otherwise, the name of the track.
        /// </summary>
        public string SequenceTrackName
        {
            get { return this._SequenceTrackName; }

            set
            {
                this.StoreText(value, this.DataIndex, this.SequenceTrackName.Length);
                this._SequenceTrackName = value;
                this.DataComment = string.Format("\"{0}\"", value);
                this.SetComment();
            }
        }

        #endregion
    }
}
