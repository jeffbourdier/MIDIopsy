/* MidiTrackChunk.cs - Implementation of MidiTrackChunk class, which represents a MIDI track (MTrk) chunk.
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

/* Dictionary */
using System.Collections.Generic;


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
        /// <param name="owner">The MIDI file to which this chunk belongs.</param>
        public MidiTrackChunk(MidiFile owner)
            : base(owner, "MTrk", 4)
        {
            MidiEndOfTrackEvent endOfTrackEvent = new MidiEndOfTrackEvent(this, 0, new byte[] { 0 }, 0);
            this.AddItem(endOfTrackEvent);
        }

        /// <summary>Initializes a new instance of the MidiTrackChunk class.</summary>
        /// <param name="owner">The MIDI file to which this chunk belongs.</param>
        /// <param name="length">Number of bytes in the chunk (not including the eight bytes of type and length).</param>
        /// <param name="bytes">Array of bytes containing the chunk data.</param>
        /// <param name="index">
        /// Index in the byte array at which the chunk data begins (not including the eight bytes of type and length).
        /// </param>
        public MidiTrackChunk(MidiFile owner, int length, byte[] bytes, int index)
            : base(owner, "MTrk", length)
        {
            int n = index + length, i, dt, j, ct = 0;
            string s;
            uint u;
            MidiEvent mtrkEvent = null;

            s = string.Format("parsing MTrk events at bytes {0} through {1}", index, n);
            Logger.WriteMessage(s);

            /* A track chunk is a stream of MIDI (MTrk) events.  Process each such event in the byte array. */
            for (i = index; i < n; i += mtrkEvent.Length)
            {
                /* Ensure that we're not trying to read past the end of our data. */
                if (i >= bytes.Length)
                {
                    s = string.Format(Properties.Resources.MismatchFormat, Properties.Resources.Byte, length, i - index);
                    Logger.WriteMessage(s);
                    this.AppendErrorText(s);
                    break;
                }

                /* Read the event's delta-time (stored as a variable-length quantity) from the byte array. */
                dt = MidiData.ReadVLQ(bytes, i);
                j = i + MidiData.SizeVLQ(dt);

                /* Instantiate an event object of the appropriate type, based on the (presumed) status byte. */
                u = MidiData.ReadNumber(bytes, 1, j);
                ++j;
                switch (u)
                {
                    case 0xFF: mtrkEvent = this.CreateMetaEvent(dt, bytes, j); break;
                    case 0xF7: mtrkEvent = new MidiSysExEvent(this, dt, true, bytes, j); break;
                    case 0xF0: mtrkEvent = new MidiSysExEvent(this, dt, false, bytes, j); break;
                    default: mtrkEvent = this.CreateChannelEvent(dt, u, bytes, j); break;
                }
                mtrkEvent.CumulativeTime = (ct += dt);
                this.AddItem(mtrkEvent);
                this.UpdateRunningChannel();
                this.UpdateKeySignatureMap();
            }

            /* The last event in a track chunk should be an End of Track meta-event. */
            if (mtrkEvent is MidiEndOfTrackEvent) return;
            Logger.WriteMessage(Properties.Resources.NoEndOfTrack);
            this.AppendErrorText(Properties.Resources.NoEndOfTrack);
        }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private Dictionary<int, uint> ChannelMap = new Dictionary<int, uint>();

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Gets the running channel at a given (cumulative) time.</summary>
        /// <param name="time">The cumulative time (in ticks) at which to get the running channel.</param>
        /// <returns>The running channel at the specified time.</returns>
        public uint GetChannel(int time)
        {
            uint channel = uint.MaxValue;

            foreach (KeyValuePair<int, uint> pair in this.ChannelMap)
            {
                if (time < pair.Key) break;
                if (channel == uint.MaxValue) channel = pair.Value;
            }
            return channel;
        }

        #endregion

        #region Private Methods

        private MidiMetaEvent CreateMetaEvent(int deltaTime, byte[] bytes, int index)
        {
            /* Read the meta-event's type byte (which must be less than 128) from the byte array. */
            uint u = MidiData.ReadNumber(bytes, 1, index);
            MidiData.ValidateNumber(u, 0, (uint)sbyte.MaxValue, Properties.Resources.MetaEventType);

            /* Instantiate a meta-event object of the appropriate type (based on the type byte). */
            switch (u)
            {
                case 0x00: return new MidiSequenceNumberEvent(this, deltaTime, bytes, index + 1);
                case 0x01: return new MidiTextEvent(this, deltaTime, bytes, index + 1);
                case 0x02: return new MidiCopyrightNoticeEvent(this, deltaTime, bytes, index + 1);
                case 0x03: return new MidiSequenceTrackNameEvent(this, deltaTime, bytes, index + 1);
                case 0x04: return new MidiInstrumentNameEvent(this, deltaTime, bytes, index + 1);
                case 0x05: return new MidiLyricEvent(this, deltaTime, bytes, index + 1);
                case 0x06: return new MidiMarkerEvent(this, deltaTime, bytes, index + 1);
                case 0x07: return new MidiCuePointEvent(this, deltaTime, bytes, index + 1);
                case 0x20: return new MidiChannelPrefixEvent(this, deltaTime, bytes, index + 1);
                case 0x2F: return new MidiEndOfTrackEvent(this, deltaTime, bytes, index + 1);
                case 0x51: return new MidiSetTempoEvent(this, deltaTime, bytes, index + 1);
                case 0x54: return new MidiSmpteOffsetEvent(this, deltaTime, bytes, index + 1);
                case 0x58: return new MidiTimeSignatureEvent(this, deltaTime, bytes, index + 1);
                case 0x59: return new MidiKeySignatureEvent(this, deltaTime, bytes, index + 1);
                case 0x7F: return new MidiSequencerSpecificMetaEvent(this, deltaTime, bytes, index + 1);
                default: return new MidiMetaEvent(this, deltaTime, u, bytes, index + 1);
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
                case 0x8: return new MidiNoteOffEvent(this, deltaTime, c, bytes, index);
                case 0x9: return new MidiNoteOnEvent(this, deltaTime, c, bytes, index);
                case 0xA: return new MidiPolyKeyPressureEvent(this, deltaTime, c, bytes, index);
                case 0xB:
                    if (bytes[index] < 120) return new MidiControlChangeEvent(this, deltaTime, c, bytes, index);
                    else return new MidiChannelModeEvent(this, deltaTime, c, bytes, index);
                case 0xC: return new MidiProgramChangeEvent(this, deltaTime, c, bytes, index);
                case 0xD: return new MidiChannelPressureEvent(this, deltaTime, c, bytes, index);
                case 0xE: return new MidiPitchBendEvent(this, deltaTime, c, bytes, index);
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
            if (lastEvent is MidiNoteOffEvent) return new MidiNoteOffEvent(this, deltaTime, bytes, index);
            if (lastEvent is MidiNoteOnEvent) return new MidiNoteOnEvent(this, deltaTime, bytes, index);
            if (lastEvent is MidiPolyKeyPressureEvent) return new MidiPolyKeyPressureEvent(this, deltaTime, bytes, index);
            if (lastEvent is MidiControlChangeEvent || lastEvent is MidiChannelModeEvent)
                if (bytes[index] < 120) return new MidiControlChangeEvent(this, deltaTime, bytes, index);
                else return new MidiChannelModeEvent(this, deltaTime, bytes, index);
            if (lastEvent is MidiProgramChangeEvent) return new MidiProgramChangeEvent(this, deltaTime, bytes, index);
            if (lastEvent is MidiChannelPressureEvent) return new MidiChannelPressureEvent(this, deltaTime, bytes, index);
            if (lastEvent is MidiPitchBendEvent) return new MidiPitchBendEvent(this, deltaTime, bytes, index);
            throw new ApplicationException(Properties.Resources.InvalidStatus);
        }

        private void UpdateRunningChannel()
        {
            MidiChannelEvent channelEvent = this.GetData(this.DataCount - 1) as MidiChannelEvent;
            if (channelEvent == null || channelEvent.RunningStatus) return;
            this.ChannelMap[channelEvent.CumulativeTime] = channelEvent.Channel;
        }

        private void UpdateKeySignatureMap()
        {
            MidiKeySignatureEvent keySignatureEvent = this.GetData(this.DataCount - 1) as MidiKeySignatureEvent;
            if (keySignatureEvent == null) return;

            if (this.File.Format > 1) this.AddKeySignature(keySignatureEvent);
            else this.File.AddKeySignature(keySignatureEvent);

            string s = string.Format("{0} {1} at {2} ticks (cumulative)", Properties.Resources.KeySignature,
                keySignatureEvent.KeySignature, keySignatureEvent.CumulativeTime);
            Logger.WriteMessage(s);
        }

        #endregion
    }
}
