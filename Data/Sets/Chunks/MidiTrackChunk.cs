/* MidiTrackChunk.cs - Implementation of MidiTrackChunk class, which represents a MIDI track (MTrk) chunk.
 *
 * Copyright (c) 2018-9 Jeffrey Paul Bourdier
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
    /// <summary>Represents a MIDI track (MTrk) chunk.</summary>
    public class MidiTrackChunk : MidiChunk
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiTrackChunk class.</summary>
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        /// <param name="bytes">Array of bytes containing the chunk data.</param>
        /// <param name="index">
        /// Index in the byte array at which the chunk data begins (not including the eight bytes of type and length).
        /// </param>
        public MidiTrackChunk(int length, byte[] bytes, int index)
            : base("MTrk", length)
        {
            int n, i, dt, j;
            uint u;
            MidiEvent mtrkEvent;

            /* A track chunk is a stream of MIDI (MTrk) events.  Process each such event in the byte array. */
            n = index + length;
            for (i = index; i < n; i += mtrkEvent.Length)
            {
                /* Read the event's delta-time (stored as a variable-length quantity) from the byte array. */
                dt = MidiData.ReadVLQ(bytes, i);
                j = i + MidiData.SizeVLQ(dt);

                /* Instantiate an event object of the appropriate type, based on the (presumed) status byte. */
                u = MidiData.ReadNumber(bytes, 1, j);
                ++j;
                switch (u)
                {
                    case 0xFF: mtrkEvent = this.CreateMetaEvent(dt, bytes, j); break;
                    case 0xF7: mtrkEvent = new MidiSysExEvent(dt, true, bytes, j); break;
                    case 0xF0: mtrkEvent = new MidiSysExEvent(dt, false, bytes, j); break;
                    default: mtrkEvent = this.CreateChannelEvent(dt, u, bytes, j); break;
                }
                this.AddItem(mtrkEvent);
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private MidiMetaEvent CreateMetaEvent(int deltaTime, byte[] bytes, int index)
        {
            /* Read the meta-event's type byte (which must be less than 128) from the byte array. */
            uint u = MidiData.ReadNumber(bytes, 1, index);
            MidiData.ValidateNumber(u, 0, (uint)sbyte.MaxValue, Properties.Resources.MetaEventType);

            /* Instantiate a meta-event object of the appropriate type (based on the type byte). */
            switch (u)
            {
                case 0x00: return new MidiSequenceNumberEvent(deltaTime, bytes, index + 1);
                case 0x01: return new MidiTextEvent(deltaTime, bytes, index + 1);
                case 0x02: return new MidiCopyrightNoticeEvent(deltaTime, bytes, index + 1);
                case 0x03: return new MidiSequenceTrackNameEvent(deltaTime, bytes, index + 1);
                case 0x04: return new MidiInstrumentNameEvent(deltaTime, bytes, index + 1);
                case 0x05: return new MidiLyricEvent(deltaTime, bytes, index + 1);
                case 0x06: return new MidiMarkerEvent(deltaTime, bytes, index + 1);
                case 0x07: return new MidiCuePointEvent(deltaTime, bytes, index + 1);
                case 0x20: return new MidiChannelPrefixEvent(deltaTime, bytes, index + 1);
                case 0x2F: return new MidiEndOfTrackEvent(deltaTime, bytes, index + 1);
                case 0x51: return new MidiSetTempoEvent(deltaTime, bytes, index + 1);
                case 0x54: return new MidiSmpteOffsetEvent(deltaTime, bytes, index + 1);
                case 0x58: return new MidiTimeSignatureEvent(deltaTime, bytes, index + 1);
                case 0x59: return new MidiKeySignatureEvent(deltaTime, bytes, index + 1);
                case 0x7F: return new MidiSequencerSpecificMetaEvent(deltaTime, bytes, index + 1);
                default: return new MidiMetaEvent(deltaTime, u, bytes, index + 1);
            }
        }

        private MidiChannelEvent CreateChannelEvent(int deltaTime, uint u, byte[] bytes, int index)
        {
            uint m, c;
            MidiData lastEvent = null;

            /* If the (presumed) status byte is an actual status byte (MSB set), the more significant
             * nibble (four bits) is the type, and the other (less significant) nibble is the channel.
             */
            m = (u >> 4) & 0xF;
            c = u & 0xF;

            /* Instantiate a MIDI channel message/event object of the
             * appropriate type (based on the type nibble of the status byte).
             */
            switch (m)
            {
                case 0x8: return new MidiNoteOffEvent(deltaTime, c, bytes, index);
                case 0x9: return new MidiNoteOnEvent(deltaTime, c, bytes, index);
                case 0xA: return new MidiPolyKeyPressureEvent(deltaTime, c, bytes, index);
                case 0xB:
                    if (bytes[index] < 120) return new MidiControlChangeEvent(deltaTime, c, bytes, index);
                    else return new MidiChannelModeEvent(deltaTime, c, bytes, index);
                case 0xC: return new MidiProgramChangeEvent(deltaTime, c, bytes, index);
                case 0xD: return new MidiChannelPressureEvent(deltaTime, c, bytes, index);
                case 0xE: return new MidiPitchBendEvent(deltaTime, c, bytes, index);
                case 0xF: throw new ApplicationException(Properties.Resources.InvalidStatus);
            }

            /* The only other possibility is the data byte of a MIDI channel message using running status,
             * in which case the index needs to be "rewound" by 1 to compensate for the absence of a
             * status byte.  (Note that this does not affect the value of the index in the caller.)
             */
            --index;

            /* Although the MIDI file spec says "Sysex events and meta-events cancel any running
             * status which was in effect," in practice, most programs allow running status even
             * when the preceding event is not a MIDI channel message, so we have to allow it here.
             */
            for (int i = this.DataCount - 1; i > 0; --i)
            {
                lastEvent = this.GetItem(i);
                if (lastEvent is MidiChannelEvent) break;
            }

            /* Instantiate a MIDI channel message/event object of the appropriate type, using running status. */
            if (lastEvent is MidiNoteOffEvent) return new MidiNoteOffEvent(deltaTime, bytes, index);
            if (lastEvent is MidiNoteOnEvent) return new MidiNoteOnEvent(deltaTime, bytes, index);
            if (lastEvent is MidiPolyKeyPressureEvent) return new MidiPolyKeyPressureEvent(deltaTime, bytes, index);
            if (lastEvent is MidiControlChangeEvent) return new MidiControlChangeEvent(deltaTime, bytes, index);
            if (lastEvent is MidiProgramChangeEvent) return new MidiProgramChangeEvent(deltaTime, bytes, index);
            if (lastEvent is MidiChannelPressureEvent) return new MidiChannelPressureEvent(deltaTime, bytes, index);
            if (lastEvent is MidiPitchBendEvent) return new MidiPitchBendEvent(deltaTime, bytes, index);
            if (lastEvent is MidiChannelModeEvent) return new MidiChannelModeEvent(deltaTime, bytes, index);
            throw new ApplicationException(Properties.Resources.InvalidStatus);
        }

        #endregion
    }
}
