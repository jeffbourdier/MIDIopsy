/* MidiEvent.cs - Implementation of MidiEvent class, which corresponds to an MTrk event in the MIDI file spec.
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
    /// <summary>Corresponds to an MTrk event in the MIDI file spec.</summary>
    public abstract class MidiEvent : MidiItem
    {
        /****************
         * Constructors *
         ****************/

        #region Protected Constructors

        /// <summary>Initializes a new instance of the MidiEvent class.</summary>
        /// <param name="file">MidiFile object representing the MIDI file to which this item belongs.</param>
        /// <param name="offset">Offset into the file's byte array at which this item begins.</param>
        protected MidiEvent(MidiFile file, int offset) : base(file, offset) { }

        #endregion

        /**********
         * Fields *
         **********/

        #region Public Fields

        /// <summary>The amount of time (in ticks) between the beginning of the file/chunk and this event.</summary>
        /// <remarks>This should be greater than or equal to the delta-time.</remarks>
        public int TotalTime = -1;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the text to display in the "Total" field (i.e., total time).</summary>
        public override string TotalField { get { return this.TotalTime.ToString(); } }

        /// <summary>Gets the text to display in the "Delta" field (i.e., delta-time).</summary>
        public override string DeltaField { get { return this.DeltaTime.ToString(); } }

        /// <summary>The amount of time (in ticks) between the previous event in the track and this one.</summary>
        public int DeltaTime
        {
            get { return this.File.ReadVLQ(this.Offset); }

            set
            {
                int deltaTime = this.DeltaTime;
                if (value == deltaTime) return;

                /* Before we set delta-time, cascade total time changes through all subsequent events in the track. */
                int i = this.File.GetItemIndex(this.Offset);
                if (this.File.GetItem(i).Offset < this.Offset) ++i;
                this.File.AdjustTotalTimes(i, value - deltaTime);

                /* Now we can set the delta-time. */
                i = Midi.SizeVLQ(deltaTime);
                this.File.WriteVLQ(this.Offset, value, i);
            }
        }

        /// <summary>Offset into the file's byte array at which this event's status byte is stored.</summary>
        public int StatusOffset { get { return this.Offset + Midi.SizeVLQ(this.DeltaTime); } }

        #endregion
    }
}
