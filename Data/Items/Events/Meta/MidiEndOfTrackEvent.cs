/* MidiEndOfTrackEvent.cs - Implementation of MidiEndOfTrackEvent class,
 * which corresponds to an End of Track meta-event in the MIDI file spec.
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
    /// <summary>Corresponds to an End of Track meta-event in the MIDI file spec.</summary>
    public sealed class MidiEndOfTrackEvent : MidiMetaEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiEndOfTrackEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        /// <remarks>This event is not optional.</remarks>
        public MidiEndOfTrackEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 0x2F, bytes, 3, index)
        {
            this.TypeComment = string.Format(Properties.Resources.MetaEventFormat,
                Properties.Resources.EndOfTrack, this.DataLength);
            this.SetComment();
        }

        #endregion
    }
}
