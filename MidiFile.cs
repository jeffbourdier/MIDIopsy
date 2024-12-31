/* MidiFile.cs - Implementation of MidiFile class, which represents a standard MIDI file.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* ApplicationException, Array, Math */
using System;

/* List, SortedDictionary */
using System.Collections.Generic;

/* File */
using System.IO;


namespace JeffBourdier
{
    /// <summary>
    /// Represents a standard MIDI file (essentially, a byte array (which
    /// may or may not be stored on disk) abstracted by MidiItem objects).
    /// </summary>
    public class MidiFile
    {
        /**********
         * Fields *
         **********/

        #region Private Fields

        private byte[] Bytes = null;
        private List<MidiItem> Items = new List<MidiItem>();
        private SortedDictionary<int, int> RunningStatusMap = new SortedDictionary<int, int>();
        private SortedDictionary<int, MidiKeySignature> KeySignatureMap = new SortedDictionary<int, MidiKeySignature>();
        private string _ErrorText = string.Empty;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of MidiItem objects contained in this file.</summary>
        public int ItemCount { get { return this.Items.Count; } }

        /// <summary>Gets a MidiHeader object representing the data of this file's MIDI header (MThd) chunk.</summary>
        public MidiHeader Header { get { return this.Items[1] as MidiHeader; } }

        /// <summary>Gets a string describing any errors associated with this file.</summary>
        public string ErrorText { get { return this._ErrorText; } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Creates a new MIDI file (represented by this instance) based on user input with metrical time.</summary>
        /// <param name="format">Specifies the overall organization of the MIDI file.</param>
        /// <param name="numberOfTracks">Number of track chunks in the MIDI file.</param>
        /// <param name="ticksPerQuarterNote">Number of delta-time "ticks" which make up a quarter-note.</param>
        /// <remarks>
        /// A header (MThd) chunk, as well as the appropriate number of track (MTrk) chunks
        /// (with End of Track events), will be created and added to the file automatically.
        /// </remarks>
        public void Create(int format, int numberOfTracks, int ticksPerQuarterNote)
        { this.Create(format, numberOfTracks).SetMetricalDivision(ticksPerQuarterNote); }

        /// <summary>
        /// Creates a new MIDI file (represented by this instance) based on user input with time-code-based time.
        /// </summary>
        /// <param name="format">Specifies the overall organization of the MIDI file.</param>
        /// <param name="numberOfTracks">Number of track chunks in the MIDI file.</param>
        /// <param name="framesPerSecond">
        /// Standard SMPTE and MIDI time code format representing the number of frames per second.
        /// </param>
        /// <param name="ticksPerFrame">Resolution within a frame (ticks per frame).</param>
        /// <remarks>
        /// A header (MThd) chunk, as well as the appropriate number of track (MTrk) chunks
        /// (with End of Track events), will be created and added to the file automatically.
        /// </remarks>
        public void Create(int format, int numberOfTracks, int framesPerSecond, int ticksPerFrame)
        { this.Create(format, numberOfTracks).SetTimeCodeBasedDivision(framesPerSecond, ticksPerFrame); }

        /// <summary>Loads an existing MIDI file from disk (into this instance).</summary>
        /// <param name="path">The path of the MIDI file to load.</param>
        public void Load(string path)
        {
            string s;
            int i, j, n = 0;
            MidiChunkInfo chunkInfo;
            MidiHeader header = null;

            /* Make sure we start with a clean slate. */
            this.Clear();
            this.Bytes = File.ReadAllBytes(path);

            /* Process each MidiItem object from the byte array. */
            for (i = 0; (j = this.Bytes.Length - i) >= MidiChunkInfo.TotalSize; i += chunkInfo.Length)
            {
                /* A chunk should begin here. */
                chunkInfo = new MidiChunkInfo(this, i);
                this.Items.Add(chunkInfo);
                i += MidiChunkInfo.TotalSize;

                /* What comes next depends on the chunk type. */
                switch (chunkInfo.Type)
                {
                    case MidiChunkInfo.HeaderType:
                        if (header == null)
                        {
                            header = new MidiHeader(this, i);
                            this.Items.Add(header);
                        }
                        else this.AddErrorText(Properties.Resources.MultipleHeaders, 0);
                        break;
                    case MidiChunkInfo.TrackType: this.ParseEvents(i, chunkInfo.Length, ++n); break;
                }
            }

            /* Check for track number mismatch. */
            i = (header == null) ? 0 : header.NumberOfTracks;
            if (i != n)
            {
                s = UI.ParseLabel(Properties.Resources.Track).ToLower();
                s = string.Format(Properties.Resources.MismatchFormat, s, i, n);
                this.AddErrorText(s, 0);
            }

            /* Check for leftover bytes or unexpected end of file. */
            if (j > 0)
            {
                s = string.Format(Properties.Resources.ExtraBytesFormat, j);
                this.AddErrorText(s, 0);
            }
            else if (j < 0) this.AddErrorText(Properties.Resources.UnexpectedEOF, 0);
        }

        /// <summary>Gets the MidiItem object at the specified index in this file's list.</summary>
        /// <param name="index">The zero-based index of the MidiItem object in the list to get.</param>
        /// <returns>The MidiItem object at the specified index in the list.</returns>
        public MidiItem GetItem(int index) { return this.Items[index]; }

        /// <summary>Represents a portion of this file's byte array in hexadecimal format.</summary>
        /// <param name="offset">Offset into the byte array at which to start reading.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>Representation of the specified portion of the byte array in hexadecimal format.</returns>
        public string FormatHex(int offset, int length) { return Midi.FormatHex(this.Bytes, offset, length); }

        /// <summary>Reads a number of bytes from this file's byte array.</summary>
        /// <param name="offset">Offset into the byte array at which to start reading.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>A byte array containing a copy of the specified data in this file's byte array.</returns>
        public byte[] ReadBytes(int offset, int length)
        {
            byte[] a = new byte[length];
            Array.Copy(this.Bytes, offset, a, 0, length);
            return a;
        }

        /// <summary>Reads text as a string of ASCII characters from this file's byte array.</summary>
        /// <param name="offset">Offset into the byte array at which the text is stored.</param>
        /// <param name="length">The number of bytes (ASCII characters) to read.</param>
        /// <returns>The text that is read.</returns>
        public string ReadText(int offset, int length) { return Midi.ReadText(this.Bytes, offset, length); }

        /// <summary>Reads a number as an integer (in network byte order) from this file's byte array.</summary>
        /// <param name="offset">
        /// Offset into the byte array at which the number is stored (most significant byte first).
        /// </param>
        /// <param name="length">The number of bytes to read (should not be greater than 4).</param>
        /// <returns>The number that is read.</returns>
        public int ReadNumber(int offset, int length) { return Midi.ReadNumber(this.Bytes, offset, length); }

        /// <summary>Reads a variable-length quantity (VLQ) from this file's byte array.</summary>
        /// <param name="offset">Offset into the byte array at which the VLQ is stored.</param>
        /// <returns>The VLQ that is read.</returns>
        public int ReadVLQ(int offset)
        {
            /* A variable-length quantity is in big-endian order, and only the lowest seven bits of each byte are part of the
             * quantity.  The Most Significant Bit (MSB) is set in all bytes except the last.  It can be up to four bytes long.
             */
            for (int n = 0, i = 0; i < 4; ++i)
            {
                /* As appropriate, initialize the result (or shift it seven bits
                 * to the left) and add to it the lowest seven bits of this byte.
                 */
                n = (i > 0) ? (n << 7) : 0;
                n += this.Bytes[offset + i] & 0x7F;

                /* If the MSB is set, we have not yet encountered the last byte of the VLQ, so continue
                 * processing.  Otherwise, we have encountered the last byte, so return the result.
                 */
                if ((this.Bytes[offset + i] & 0x80) > 0) continue;
                return n;
            }

            /* This means we processed four bytes and never encountered the last byte (MSB clear). */
            throw new ApplicationException(Properties.Resources.InvalidVLQ);
        }

        /// <summary>Writes a number of bytes into this file's byte array, resizing the byte array if necessary.</summary>
        /// <param name="offset">Offset into the byte array at which the bytes are written.</param>
        /// <param name="bytes">Array of bytes to write.</param>
        /// <param name="length">Length of data that the bytes are replacing (for resizing purposes).</param>
        public void WriteBytes(int offset, byte[] bytes, int length)
        {
            int n = ((bytes == null) ? 0 : bytes.Length) - length;
            if (n != 0) this.Resize(n, offset + length, 0);
            if (bytes == null) return;
            Array.Copy(bytes, 0, this.Bytes, offset, bytes.Length);
        }

        /// <summary>
        /// Writes text as a string of ASCII characters into this file's byte array, resizing the byte array if necessary.
        /// </summary>
        /// <param name="offset">Offset into the byte array at which the text is written.</param>
        /// <param name="text">The text to write.</param>
        /// <param name="length">Length of string that the text is replacing (for resizing purposes).</param>
        public void WriteText(int offset, string text, int length)
        {
            int n = ((text == null) ? 0 : text.Length) - length;
            if (n != 0) this.Resize(n, offset + length, 0);
            if (text == null) return;
            for (int i = 0; i < text.Length; ++i) this.Bytes[offset + i] = (byte)text[i];
        }

        /// <summary>Writes a number as an integer (in network byte order) into this file's byte array.</summary>
        /// <param name="offset">
        /// Offset into the byte array at which the number is written (most significant byte first).
        /// </param>
        /// <param name="n">The number to write.</param>
        /// <param name="length">The number of bytes to write (should not be greater than 4).</param>
        public void WriteNumber(int offset, int n, int length)
        { for (int i = 0; i < length; ++i) this.Bytes[offset + i] = (byte)(0xFF & (n >> (8 * (length - i - 1)))); }

        /// <summary>
        /// Writes a variable-length quantity (VLQ) into this file's byte array, resizing the byte array if necessary.
        /// </summary>
        /// <param name="offset">Offset into the byte array at which the VLQ is written.</param>
        /// <param name="vlq">The VLQ to write.</param>
        /// <param name="length">Length of quantity that the VLQ is replacing (for resizing purposes).</param>
        public void WriteVLQ(int offset, int vlq, int length)
        {
            /* Resize the byte array (if necessary). */
            int i, n = Midi.SizeVLQ(vlq) - length;
            if (n != 0) this.Resize(n, offset + length, 0);

            /* For the simple (and common) case of a single-byte VLQ, take a shortcut. */
            if (vlq < 0x80) { this.Bytes[offset] = (byte)vlq; return; }

            /* The VLQ is stored as 7 bits per byte (most significant byte first).  All bytes except the last have the
             * Most Significant Bit (MSB) set, and the last byte has the MSB clear.  The easiest way to accomplish this
             * is to write the VLQ "backwards" (least significant byte first) to a temporary byte array, then reverse it.
             */
            for (n = 0, i = vlq; i > 0; i >>= 7)
            {
                this.Bytes[offset + n] = (byte)((i % 0x100) & 0x7F);
                if (n > 0) this.Bytes[offset + n] |= 0x80;
                ++n;
            }
            Array.Reverse(this.Bytes, offset, n);
        }

        /// <summary>Adds a new track (MTrk) chunk to the end of this file.</summary>
        /// <returns>The new MidiChunkInfo object that is added.</returns>
        public MidiChunkInfo AddTrack()
        {
            int n = MidiChunkInfo.TotalSize, offset = this.Bytes.Length;
            this.Resize(n, offset, -1);
            MidiChunkInfo chunkInfo = this.CreateChunkInfo(offset, MidiChunkInfo.TrackType);
            this.Items.Add(chunkInfo);
            return chunkInfo;
        }

        /// <summary>Adds a new MIDI channel message/event to the end of this file.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="status">The status byte of the event, or -1 for running status.</param>
        /// <param name="data1">The first data byte of the event.</param>
        /// <param name="data2">The second data byte of the event (if applicable).</param>
        /// <returns>The new MidiChannelEvent object that is added.</returns>
        public MidiChannelEvent AddChannelEvent(int deltaTime, int status, int data1, int data2)
        {
            int offset = this.Bytes.Length, n = (status < 0) ? this.GetRunningStatus(offset) : status;
            MidiMessageType messageType = (MidiMessageType)Midi.GetHighNibble(n);
            n = MidiChannelEvent.TotalSize(0, (status < 0), messageType);
            this.Resize(n, offset, this.ItemCount);
            MidiChannelEvent channelEvent = this.CreateChannelEvent(offset, deltaTime, status, data1, data2);
            this.Items.Add(channelEvent);
            this.SetTotalTime(this.ItemCount - 1);
            return channelEvent;
        }

        /// <summary>Adds a new system exclusive (SysEx) message/event to the end of this file.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="escape">
        /// False indicates a regular SysEx message, which could be the first in a series of timed packets.
        /// True indicates a SysEx "escape," or the next in a series of timed packets.
        /// </param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <returns>The new MidiSysExEvent object that is added.</returns>
        public MidiSysExEvent AddSysExEvent(int deltaTime, bool escape, byte[] bytes)
        {
            int n = MidiSysExEvent.TotalSize(0, 0), offset = this.Bytes.Length;
            this.Resize(n, offset, this.ItemCount);
            MidiSysExEvent sysExEvent = this.CreateSysExEvent(offset, deltaTime, escape, bytes);
            this.Items.Add(sysExEvent);
            this.SetTotalTime(this.ItemCount - 1);
            return sysExEvent;
        }

        /// <summary>Adds a new meta-event to the end of this file.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="type">Meta-event type (always less than 128).</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <returns>The new MidiMetaEvent object that is added.</returns>
        public MidiMetaEvent AddMetaEvent(int deltaTime, int type, byte[] bytes)
        {
            int n = MidiMetaEvent.TotalSize(0, 0), offset = this.Bytes.Length;
            this.Resize(n, offset, this.ItemCount);
            MidiMetaEvent metaEvent = this.CreateMetaEvent(offset, deltaTime, type, bytes);
            this.Items.Add(metaEvent);
            this.SetTotalTime(this.ItemCount - 1);
            return metaEvent;
        }

        /// <summary>Inserts a new track (MTrk) chunk into this file at the specified index.</summary>
        /// <param name="index">The index in this file's list at which the track chunk should be inserted.</param>
        /// <returns>The new MidiChunkInfo object that is inserted.</returns>
        public MidiChunkInfo InsertTrack(int index)
        {
            int n = MidiChunkInfo.TotalSize, offset = this.Items[index].Offset;
            this.Resize(n, offset, -1);
            MidiChunkInfo chunkInfo = this.CreateChunkInfo(offset, MidiChunkInfo.TrackType);
            this.Items.Insert(index, chunkInfo);
            return chunkInfo;
        }

        /// <summary>Inserts a new MIDI channel message/event into this file at the specified index.</summary>
        /// <param name="index">The index in this file's list at which the event should be inserted.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="status">The status byte of the event, or -1 for running status.</param>
        /// <param name="data1">The first data byte of the event.</param>
        /// <param name="data2">The second data byte of the event (if applicable).</param>
        /// <returns>The new MidiChannelEvent object that is inserted.</returns>
        public MidiChannelEvent InsertChannelEvent(int index, int deltaTime, int status, int data1, int data2)
        {
            int offset = this.Items[index].Offset, n = (status < 0) ? this.GetRunningStatus(offset) : status;
            MidiMessageType messageType = (MidiMessageType)Midi.GetHighNibble(n);
            n = MidiChannelEvent.TotalSize(0, (status < 0), messageType);
            this.Resize(n, offset, index);
            MidiChannelEvent channelEvent = this.CreateChannelEvent(offset, deltaTime, status, data1, data2);
            this.Items.Insert(index, channelEvent);
            this.SetTotalTime(index);
            return channelEvent;
        }

        /// <summary>Inserts a new system exclusive (SysEx) message/event into this file at the specified index.</summary>
        /// <param name="index">The index in this file's list at which the event should be inserted.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="escape">
        /// False indicates a regular SysEx message, which could be the first in a series of timed packets.
        /// True indicates a SysEx "escape," or the next in a series of timed packets.
        /// </param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <returns>The new MidiSysExEvent object that is inserted.</returns>
        public MidiSysExEvent InsertSysExEvent(int index, int deltaTime, bool escape, byte[] bytes)
        {
            int n = MidiSysExEvent.TotalSize(0, 0), offset = this.Items[index].Offset;
            this.Resize(n, offset, index);
            MidiSysExEvent sysExEvent = this.CreateSysExEvent(offset, deltaTime, escape, bytes);
            this.Items.Insert(index, sysExEvent);
            this.SetTotalTime(index);
            return sysExEvent;
        }

        /// <summary>Inserts a new meta-event into this file at the specified index.</summary>
        /// <param name="index">The index in this file's list at which the event should be inserted.</param>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="type">Meta-event type (always less than 128).</param>
        /// <param name="bytes">
        /// Array of bytes containing the event data (not including the delta-time, status byte, or type byte).
        /// </param>
        /// <returns>The new MidiMetaEvent object that is inserted.</returns>
        public MidiMetaEvent InsertMetaEvent(int index, int deltaTime, int type, byte[] bytes)
        {
            int n = MidiMetaEvent.TotalSize(0, 0), offset = this.Items[index].Offset;
            this.Resize(n, offset, index);
            MidiMetaEvent metaEvent = this.CreateMetaEvent(offset, deltaTime, type, bytes);
            this.Items.Insert(index, metaEvent);
            this.SetTotalTime(index);
            return metaEvent;
        }

        /// <summary>Removes from this file the MidiItem object at the specified index.</summary>
        /// <param name="index">The index in this file's list of the MidiItem object to remove.</param>
        public void RemoveItem(int index)
        {
            /* Determine how many bytes to remove from the array.  If the item is chunk info, the entire
             * chunk will be removed.  If it is a track (MTrk) chunk, decrement the number of tracks.
             */
            MidiItem item = this.Items[index];
            int i, n = item.Size;
            MidiChunkInfo chunkInfo = item as MidiChunkInfo;
            if (chunkInfo != null)
            {
                n += chunkInfo.Length;
                i = this.Bytes.Length - item.Offset;
                if (n > i) n = i;
                if (chunkInfo.Type == MidiChunkInfo.TrackType) --this.Header.NumberOfTracks;
            }

            /* If the item is an MTrk event with a nonzero delta-time, the total time of each subsequent
             * event in the track/chunk will need to be adjusted accordingly (after this item is removed).
             */
            MidiEvent mtrkEvent = item as MidiEvent;
            int deltaTime = (mtrkEvent == null) ? 0 : mtrkEvent.DeltaTime;

            /* Remove from the list all items whose offsets are within the range of data being removed. */
            for (i = item.Offset + n; index < this.ItemCount && this.Items[index].Offset < i; this.Items.RemoveAt(index))
            {
                item = this.Items[index];

                /* If the item being removed is a channel message/event that does not use running status, there
                 * should be a corresponding entry in the map of running statuses that needs to be removed.
                 */
                MidiChannelEvent channelEvent = item as MidiChannelEvent;
                if (channelEvent != null && !channelEvent.RunningStatus) this.RunningStatusMap.Remove(item.Offset);

                /* If the item being removed is a meta-event representing a key signature, there should
                 * be a corresponding entry in the key signature map that also needs to be removed.
                 */
                MidiMetaEvent metaEvent = MidiFile.ItemToKeySignatureEvent(item);
                if (metaEvent != null) this.KeySignatureMap.Remove(metaEvent.TotalTime);
            }

            /* If applicable, cascade total time changes through all subsequent events in the track. */
            this.AdjustTotalTimes(index, -deltaTime);

            /* Remove the appropriate number of bytes from the array. */
            this.Resize(-n, i, (chunkInfo == null) ? index : -1);
        }

        /// <summary>Adds an entry to the map of running statuses, or updates the entry if it already exists.</summary>
        /// <param name="offset">Offset into the byte array at which to set the running status.</param>
        /// <param name="status">Running status byte.</param>
        public void SetRunningStatus(int offset, int status) { this.RunningStatusMap[offset] = status; }

        /// <summary>Gets the running status in effect at a given offset.</summary>
        /// <param name="offset">Offset into the byte array at which to get the effective running status.</param>
        /// <returns>The running status in effect at the given offset, or -1 if there is none.</returns>
        public int GetRunningStatus(int offset)
        {
            int status = -1;
            foreach (KeyValuePair<int, int> pair in this.RunningStatusMap)
            {
                if (pair.Key > offset) break;
                status = pair.Value;
            }
            return status;
        }

        /// <summary>Adds an entry to the key signature map, or updates the entry if it already exists.</summary>
        /// <param name="metaEvent">A MidiMetaEvent object representing a key signature.</param>
        public void SetKeySignature(MidiMetaEvent metaEvent)
        {
            if (metaEvent.TotalTime < 0) return;
            MidiKeySignature keySignature = MidiMetaEvent.DataToKeySignature(metaEvent.Data);
            if (keySignature == MidiKeySignature.NA) return;
            this.KeySignatureMap[metaEvent.TotalTime] = keySignature;
        }

        /// <summary>Gets the key signature in effect at a given (total) time.</summary>
        /// <param name="time">The total time (in ticks) at which to get the effective key signature.</param>
        /// <returns>The key signature at the specified time.</returns>
        public MidiKeySignature GetKeySignature(int time)
        {
            MidiKeySignature keySignature = MidiKeySignature.NA;
            foreach (KeyValuePair<int, MidiKeySignature> pair in this.KeySignatureMap)
            {
                if (pair.Key > time) break;
                keySignature = pair.Value;
            }
            return keySignature;
        }

        /// <summary>
        /// Changes the total (cumulative) time of each MTrk event in a track chunk, starting
        /// at the specified index in this file's list, by the specified number of ticks.
        /// </summary>
        /// <param name="index">The index of the event in this file's list at which to start the change.</param>
        /// <param name="delta">The number of ticks by which to change the total time of each event.</param>
        public void AdjustTotalTimes(int index, int delta)
        {
            if (delta == 0) return;

            /* Starting at the specified index, change the total time of each event in the list (until end-
             * of-list or a non-event is encountered, presumably indicating the end of the track/chunk),
             * and collect a list of key signature map "keys" (i.e., total times) affected by the change.
             */
            List<int> keys = new List<int>();
            for (int i = index; i < this.ItemCount; ++i)
            {
                MidiEvent mtrkEvent = this.Items[i] as MidiEvent;
                if (mtrkEvent == null) break;
                mtrkEvent.TotalTime += delta;

                /* If this is a meta-event representing a key signature, add its former
                 * total time to the list of keys (total times) affected by this change.
                 */
                MidiMetaEvent metaEvent = MidiFile.ItemToKeySignatureEvent(mtrkEvent);
                if (metaEvent != null) keys.Add(metaEvent.TotalTime - delta);
            }

            /* Since our key signature map is keyed by total time, it must be adjusted correspondingly.
             * Replace each affected entry with a new one having the updated "key" (total time).
             */
            if (delta > 0) keys.Reverse();
            foreach (int key in keys)
            {
                MidiKeySignature keySignature = this.KeySignatureMap[key];
                this.KeySignatureMap.Remove(key);
                this.KeySignatureMap[key + delta] = keySignature;
            }
        }

        /// <summary>Resizes this file's byte array, preserving the data from a given offset to the end.</summary>
        /// <param name="delta">The number of bytes by which to resize the array.</param>
        /// <param name="offset">Offset into the byte array at which to begin preservation.</param>
        /// <param name="index">
        /// Optional (non-negative) index of the affected MidiItem object in this file's list (or zero to
        /// determine the index dynamatically), so that the length of the containing chunk can be adjusted.
        /// </param>
        public void Resize(int delta, int offset, int index)
        {
            if (delta == 0) return;

            int i, j, m, n;

            if (delta < 0)
            {
                m = offset;
                n = this.Bytes.Length;
            }
            else
            {
                m = this.Bytes.Length - 1;
                n = offset - 1;
                Array.Resize(ref this.Bytes, this.Bytes.Length + delta);
            }
            for (j = delta / Math.Abs(delta), i = m; i != n; i -= j)
            {
                this.Bytes[i + delta] = this.Bytes[i];
                this.Bytes[i] = 0;
            }
            if (delta < 0) Array.Resize(ref this.Bytes, this.Bytes.Length + delta);

            /* If applicable, cascade offset changes through all subsequent items. */
            j = offset + delta;
            if (j < this.Bytes.Length)
            {
                for (i = this.GetItemIndex(offset); i < this.ItemCount; ++i) this.Items[i].Offset += delta;

                /* Since our map of running statuses is keyed by offset, it must also be adjusted correspondingly. */
                List<int> keys = new List<int>();

                /* Collect a list of keys (offsets) affected by this change. */
                foreach (int key in this.RunningStatusMap.Keys) if (key >= offset) keys.Add(key);
                if (delta > 0) keys.Reverse();

                /* Replace each affected entry in the map with a new one having the updated key (offset). */
                foreach (int key in keys)
                {
                    int status = this.RunningStatusMap[key];
                    this.RunningStatusMap.Remove(key);
                    if (delta < 0 && key < j) continue;
                    this.RunningStatusMap[key + delta] = status;
                }
            }

            /* If specified, adjust the length of the containing chunk. */
            if (index < 0) return;
            MidiChunkInfo chunkInfo = null;
            for (i = (index > 0) ? index : this.GetItemIndex((delta < 0) ? j : offset);
                chunkInfo == null; chunkInfo = this.Items[--i] as MidiChunkInfo) ;
            chunkInfo.Length += delta;
        }

        /// <summary>Gets the index in this file's list of the MidiItem object relevant to the specified offset.</summary>
        /// <param name="offset">Offset into this file's byte array.</param>
        /// <returns>The zero-based index of the relevant MidiItem object.</returns>
        public int GetItemIndex(int offset)
        {
            int i = 0;
            while (i < this.ItemCount && this.Items[i].Offset < offset) ++i;
            return i;
        }

        /// <summary>Writes this MIDI file to disk at the specified path.</summary>
        /// <param name="path">The path at which to write the MIDI file.</param>
        public void Save(string path) { File.WriteAllBytes(path, this.Bytes); }

        /// <summary>Removes all data from this MIDI file.</summary>
        public void Clear()
        {
            this._ErrorText = string.Empty;
            this.KeySignatureMap.Clear();
            this.RunningStatusMap.Clear();
            this.Items.Clear();
            this.Bytes = null;
        }

        #endregion

        #region Private Methods

        /* Create a new MIDI file (represented by this instance), including the header chunk and appropriate
         * number of track chunks (with End of Track events).  This does not set delta-time division.
         */
        private MidiHeader Create(int format, int numberOfTracks)
        {
            /* Make sure we start with a clean slate. */
            this.Clear();
            int n = MidiChunkInfo.TotalSize;
            this.Bytes = new byte[n + MidiHeader.TotalSize];

            /* Start the header chunk. */
            MidiChunkInfo chunkInfo = this.CreateChunkInfo(0, MidiChunkInfo.HeaderType);
            this.Items.Add(chunkInfo);

            /* Finish the header chunk.  (Don't set NumberOfTracks here; AddTrack will set it.) */
            MidiHeader header = new MidiHeader(this, n);
            header.Format = format;
            this.Items.Add(header);

            /* Add each track chunk (with an End of Track meta-event). */
            for (int i = 0; i < numberOfTracks; ++i)
            {
                this.AddTrack();
                this.AddMetaEvent(0, MidiMetaEvent.EndOfTrackType, null);
            }

            /* Delta-time division still needs to be set. */
            return header;
        }

        private void ParseEvents(int offset, int length, int trackNumber)
        {
            string s;
            int n = offset + length, i, j = 0;
            MidiEvent mtrkEvent = null;
            MidiMetaEvent metaEvent;

            /* A track chunk is a stream of MIDI (MTrk) events.  Process each such
             * event in this part of the byte array comprising the track chunk.
             */
            for (i = offset; i < n; i += mtrkEvent.Size)
            {
                /* In order to know what kind of event to instantiate, we must read the event's status
                 * byte, which requires skipping past the delta-time (stored as a variable-length quantity).
                 */
                for (j = i; j < this.Bytes.Length && (this.Bytes[j] & 0x80) > 0; ++j) if (j - i > 3)
                    {
                        s = string.Format("{0} (@ {1} {2})", Properties.Resources.InvalidVLQ, Properties.Resources.Byte, i);
                        this.AddErrorText(s, trackNumber);
                        return;
                    }
                if (++j >= this.Bytes.Length) break;

                /* Instantiate an event object of the appropriate type (based on the status byte). */
                switch (this.Bytes[j])
                {
                    case 0xFF: mtrkEvent = new MidiMetaEvent(this, i); break;
                    case 0xF7: mtrkEvent = new MidiSysExEvent(this, i); break;
                    case 0xF0: mtrkEvent = new MidiSysExEvent(this, i); break;
                    default: mtrkEvent = new MidiChannelEvent(this, i); break;
                }
                this.Items.Add(mtrkEvent);
                this.SetTotalTime(this.ItemCount - 1);

                /* If the event is a MIDI channel message/event that does not use
                 * running status, use it to set the running status at this byte offset.
                 */
                if (++j >= this.Bytes.Length) break;
                MidiChannelEvent channelEvent = mtrkEvent as MidiChannelEvent;
                if (channelEvent != null && !channelEvent.RunningStatus) this.SetRunningStatus(i, channelEvent.Status);

                /* If the event is a meta-event representing a key signature,
                 * use it to set the key signature at the appropriate time.
                 */
                if (++j >= this.Bytes.Length) break;
                metaEvent = MidiFile.ItemToKeySignatureEvent(mtrkEvent);
                if (metaEvent != null) this.SetKeySignature(metaEvent);
            }

            /* If we ran out of data, add an error message. */
            if (j >= this.Bytes.Length)
            {
                s = string.Format(Properties.Resources.MismatchFormat, Properties.Resources.Byte, length, i - offset);
                this.AddErrorText(s, trackNumber);
            }

            /* The last event in a track chunk should be an End of Track meta-event. */
            metaEvent = mtrkEvent as MidiMetaEvent;
            if (metaEvent == null || (metaEvent != null && metaEvent.Type != MidiMetaEvent.EndOfTrackType))
                this.AddErrorText(Properties.Resources.NoEndOfTrack, trackNumber);
        }

        private void AddErrorText(string text, int trackNumber)
        {
            string s;

            /* If a track number was supplied, include that in the error message. */
            if (trackNumber > 0)
            {
                s = UI.ParseLabel(Properties.Resources.Track);
                s = string.Format("{0} {1}:  {2}", s, trackNumber, text);
            }
            else s = text;

            /* Add the line to this file's error text. */
            if (!string.IsNullOrEmpty(this.ErrorText)) this._ErrorText += Environment.NewLine;
            this._ErrorText += s;
        }

        private MidiChunkInfo CreateChunkInfo(int offset, string type)
        {
            MidiChunkInfo chunkInfo = new MidiChunkInfo(this, offset);
            chunkInfo.Type = type;
            chunkInfo.Length = (type == MidiChunkInfo.HeaderType) ? MidiHeader.TotalSize : 0;
            if (type == MidiChunkInfo.TrackType) ++this.Header.NumberOfTracks;
            return chunkInfo;
        }

        private MidiChannelEvent CreateChannelEvent(int offset, int deltaTime, int status, int data1, int data2)
        {
            MidiChannelEvent channelEvent = new MidiChannelEvent(this, offset);
            channelEvent.DeltaTime = deltaTime;
            if (status >= 0)
            {
                this.Bytes[channelEvent.StatusOffset] = 0xF0;
                channelEvent.Status = status;
            }
            channelEvent.Data1 = data1;
            channelEvent.Data2 = data2;
            return channelEvent;
        }

        private MidiSysExEvent CreateSysExEvent(int offset, int deltaTime, bool escape, byte[] bytes)
        {
            MidiSysExEvent sysExEvent = new MidiSysExEvent(this, offset);
            sysExEvent.DeltaTime = deltaTime;
            sysExEvent.Escape = escape;
            sysExEvent.Data = bytes;
            return sysExEvent;
        }

        private MidiMetaEvent CreateMetaEvent(int offset, int deltaTime, int type, byte[] bytes)
        {
            MidiMetaEvent metaEvent = new MidiMetaEvent(this, offset);
            this.Bytes[metaEvent.StatusOffset] = 0xFF;
            metaEvent.DeltaTime = deltaTime;
            metaEvent.Type = type;
            metaEvent.Data = bytes;
            return metaEvent;
        }

        private void SetTotalTime(int index)
        {
            MidiEvent mtrkEvent = this.Items[index] as MidiEvent, previousEvent = this.Items[index - 1] as MidiEvent;
            mtrkEvent.TotalTime = mtrkEvent.DeltaTime + ((previousEvent == null) ? 0 : previousEvent.TotalTime);

            /* If the event is a meta-event representing a key signature,
             * use it to set the key signature at the appropriate time.
             */
            MidiMetaEvent metaEvent = MidiFile.ItemToKeySignatureEvent(mtrkEvent);
            if (metaEvent != null) this.SetKeySignature(metaEvent);
        }

        private static MidiMetaEvent ItemToKeySignatureEvent(MidiItem item)
        {
            MidiMetaEvent metaEvent = item as MidiMetaEvent;
            if (metaEvent == null) return null;
            return (metaEvent.Type == MidiMetaEvent.KeySignatureType) ? metaEvent : null;
        }

        #endregion
    }
}
