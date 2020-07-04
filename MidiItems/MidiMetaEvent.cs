/* MidiMetaEvent.cs - Implementation of MidiMetaEvent class, which corresponds to a meta-event in the MIDI file spec.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Enum, Math */
using System;

/* Dictionary */
using System.Collections.Generic;


namespace JeffBourdier
{
    /// <summary>
    /// Specifies enumerated constants to define valid values for the Key Signature meta-event in the MIDI file spec.
    /// </summary>
    public enum MidiKeySignature                      //     Key Signature       0   1   2   3   4   5   6   7   8   9  10  11
    {                                                 //  --------------------  --  --  --  --  --  --  --  --  --  --  --  --
        CFlatMajor  = 0xF900,  AFlatMinor  = 0xF901,  //  Bb Eb Ab Db Gb Cb Fb  Cb  C   Db  D   Eb  Fb  F   Gb  G   Ab  A   Bb
        GFlatMajor  = 0xFA00,  EFlatMinor  = 0xFA01,  //  Bb Eb Ab Db Gb Cb     Gb  G   Ab  A   Bb  Cb  C   Db  D   Eb  E   F
        DFlatMajor  = 0xFB00,  BFlatMinor  = 0xFB01,  //  Bb Eb Ab Db Gb        Db  D   Eb  E   F   Gb  G   Ab  A   Bb  B   C
        AFlatMajor  = 0xFC00,  FMinor      = 0xFC01,  //  Bb Eb Ab Db           Ab  A   Bb  B   C   Db  D   Eb  E   F   Gb  G
        EFlatMajor  = 0xFD00,  CMinor      = 0xFD01,  //  Bb Eb Ab              Eb  E   F   Gb  G   Ab  A   Bb  B   C   Db  D
        BFlatMajor  = 0xFE00,  GMinor      = 0xFE01,  //  Bb Eb                 Bb  B   C   Db  D   Eb  E   F   Gb* G   Ab  A
        FMajor      = 0xFF00,  DMinor      = 0xFF01,  //  Bb                    F   F#  G   Ab  A   Bb  B   C   Db* D   Eb  E
        CMajor      = 0x0000,  AMinor      = 0x0001,  //                        C   C#  D   Eb  E   F   F#  G   Ab* A   Bb  B
        GMajor      = 0x0100,  EMinor      = 0x0101,  //  F#                    G   G#  A   Bb  B   C   C#  D   Eb* E   F   F#
        DMajor      = 0x0200,  BMinor      = 0x0201,  //  F# C#                 D   D#  E   F   F#  G   G#  A   Bb* B   C   C#
        AMajor      = 0x0300,  FSharpMinor = 0x0301,  //  F# C# G#              A   A#  B   C   C#  D   D#  E   F*  F#  G   G#
        EMajor      = 0x0400,  CSharpMinor = 0x0401,  //  F# C# G# D#           E   E#  F#  G   G#  A   A#  B   C*  C#  D   D#
        BMajor      = 0x0500,  GSharpMinor = 0x0501,  //  F# C# G# D# A#        B   B#  C#  D   D#  E   E#  F#  G   G#  A   A#
        FSharpMajor = 0x0600,  DSharpMinor = 0x0601,  //  F# C# G# D# A# E#     F#  G   G#  A   A#  B   B#  C#  D   D#  E   E#
        CSharpMajor = 0x0700,  ASharpMinor = 0x0701,  //  F# C# G# D# A# E# B#  C#  D   D#  E   E#  F#  G   G#  A   A#  B   B#
        NA = -1                                       //                                 * Notate the other way for minor keys
    }

    /// <summary>Corresponds to a meta-event in the MIDI file spec.</summary>
    public class MidiMetaEvent : MidiEvent
    {
        /*********
         * Types *
         *********/

        #region Private Types

        /// <summary>Describes the data bytes of a meta-event.</summary>
        /// <param name="bytes">The data bytes of the event.</param>
        /// <returns>A description of the data bytes.</returns>
        private delegate string DescribeData(byte[] bytes);

        /// <summary>Contains information on a meta-event type.</summary>
        private struct TypeInfo
        {
            public TypeInfo(string comment, int minLength, DescribeData describeData)
            {
                this._Comment = comment;
                this._DataLength = minLength;
                this._DescribeData = describeData;
            }

            private string _Comment;
            private int _DataLength;
            private DescribeData _DescribeData;

            public string Comment { get { return this._Comment; } }
            public int DataLength { get { return this._DataLength; } }
            public DescribeData DescribeData { get { return this._DescribeData; } }
        }

        #endregion

        /****************
         * Constructors *
         ****************/

        #region Static Constructors

        /// <summary>Initializes static class members.</summary>
        static MidiMetaEvent()
        {
            MidiMetaEvent.TypeMap = new Dictionary<int, TypeInfo>(16);
            MidiMetaEvent.TypeMap.Add(0x00, new TypeInfo(Properties.Resources.SequenceNumber, 2, MidiMetaEvent.DescribeNumber));
            MidiMetaEvent.TypeMap.Add(0x01, new TypeInfo(Properties.Resources.TextEvent, -1, null));
            MidiMetaEvent.TypeMap.Add(0x02, new TypeInfo(Properties.Resources.CopyrightNotice, -1, null));
            MidiMetaEvent.TypeMap.Add(0x03, new TypeInfo(Properties.Resources.SequenceTrackName, -1, null));
            MidiMetaEvent.TypeMap.Add(0x04, new TypeInfo(Properties.Resources.InstrumentName, -1, null));
            MidiMetaEvent.TypeMap.Add(0x05, new TypeInfo(Properties.Resources.Lyric, -1, null));
            MidiMetaEvent.TypeMap.Add(0x06, new TypeInfo(Properties.Resources.Marker, -1, null));
            MidiMetaEvent.TypeMap.Add(0x07, new TypeInfo(Properties.Resources.CuePoint, -1, null));
            MidiMetaEvent.TypeMap.Add(0x20, new TypeInfo(Properties.Resources.MidiChannelPrefix, 1, MidiMetaEvent.DescribeNumber));
            MidiMetaEvent.TypeMap.Add(0x2F, new TypeInfo(Properties.Resources.EndOfTrack, 0, null));
            MidiMetaEvent.TypeMap.Add(0x51, new TypeInfo(Properties.Resources.SetTempo, 3, MidiMetaEvent.DescribeTempo));
            MidiMetaEvent.TypeMap.Add(0x54, new TypeInfo(Properties.Resources.SmpteOffset, 5, MidiMetaEvent.DescribeSmpteOffset));
            MidiMetaEvent.TypeMap.Add(0x58, new TypeInfo(Properties.Resources.TimeSignature, 4, MidiMetaEvent.DescribeTimeSignature));
            MidiMetaEvent.TypeMap.Add(0x59, new TypeInfo(Properties.Resources.KeySignature, 2, MidiMetaEvent.DescribeKeySignature));
            MidiMetaEvent.TypeMap.Add(0x7F, new TypeInfo(Properties.Resources.SequencerSpecificMetaEvent, -1, null));
        }

        #endregion

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiMetaEvent class.</summary>
        /// <param name="file">MidiFile object representing the MIDI file to which this item belongs.</param>
        /// <param name="offset">Offset into the file's byte array at which this item begins.</param>
        public MidiMetaEvent(MidiFile file, int offset) : base(file, offset) { }

        #endregion

        /**********
         * Fields *
         **********/

        #region Public Fields

        public const int SequenceTrackNameType = 0x03;
        public const int EndOfTrackType = 0x2F;
        public const int KeySignatureType = 0x59;

        #endregion

        #region Private Fields

        private static readonly Dictionary<int, TypeInfo> TypeMap;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes represented by this item.</summary>
        public override int Size { get { return MidiMetaEvent.SizeItem(this.DeltaTime, this.DataLength); } }

        /// <summary>Gets the text to display in the "Type" field (i.e., meta-event type).</summary>
        public override string TypeField { get { return MidiMetaEvent.GetTypeComment(this.Type); } }

        /// <summary>Gets the text to display in the "Count" field (i.e., data length).</summary>
        public override string CountField { get { return Text.FormatCount(this.DataLength, Properties.Resources.Byte); } }

        /// <summary>Gets the text to display in the "Data" field.</summary>
        public override string DataField { get { return MidiMetaEvent.GetDataComment(this.Type, this.Data); } }

        /// <summary>Meta-event type (always less than 128).</summary>
        public int Type
        {
            get { return this.File.ReadNumber(this.TypeOffset, 1); }

            set
            {
                /* If this meta-event is changing to a key signature, add an entry to the key signature map. */
                if (this.Type != MidiMetaEvent.KeySignatureType && value == MidiMetaEvent.KeySignatureType)
                    this.File.SetKeySignature(this);

                this.File.WriteNumber(this.TypeOffset, value, 1);
            }
        }

        /// <summary>The data bytes of the event.</summary>
        public byte[] Data
        {
            get { return this.File.ReadBytes(this.DataOffset, this.DataLength); }
            set
            {
                this.File.WriteBytes(this.DataOffset, value, this.DataLength);
                this.DataLength = (value == null) ? 0 : value.Length;

                /* If this meta-event represents a key signature, update its entry in the key signature map. */
                if (this.Type == MidiMetaEvent.KeySignatureType) this.File.SetKeySignature(this);
            }
        }

        #endregion

        #region Private Properties

        /* Number of bytes which follow the data length (not including the data length itself). */
        private int DataLength
        {
            get { return this.File.ReadVLQ(this.DataLengthOffset); }
            set
            {
                int n = Midi.SizeVLQ(this.DataLength);
                this.File.WriteVLQ(this.DataLengthOffset, value, n);
            }
        }

        private int TypeOffset { get { return this.StatusOffset + 1; } }
        private int DataLengthOffset { get { return this.TypeOffset + 1; } }
        private int DataOffset { get { return this.DataLengthOffset + Midi.SizeVLQ(this.DataLength); } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Returns the number of bytes required to store an item of this type.</summary>
        public static int SizeItem(int deltaTime, int dataLength)
        { return Midi.SizeVLQ(deltaTime) + 2 + Midi.SizeVLQ(dataLength) + dataLength; }

        /// <summary>Returns a general comment on a meta-event type.</summary>
        /// <param name="type">Meta-event type (always less than 128).</param>
        /// <returns>Comment on the meta-event type.</returns>
        public static string GetTypeComment(int type)
        {
            return MidiMetaEvent.TypeMap.ContainsKey(type) ?
                MidiMetaEvent.TypeMap[type].Comment : (Properties.Resources.MetaEvent + " " + type);
        }

        /// <summary>Returns the required data length for a given meta-event type.</summary>
        /// <param name="type">Meta-event type (always less than 128).</param>
        /// <returns>Required data length for the meta-event type, or -1 if no requirement.</returns>
        public static int GetDataLength(int type)
        { return MidiMetaEvent.TypeMap.ContainsKey(type) ? MidiMetaEvent.TypeMap[type].DataLength : -1; }

        /// <summary>Returns a comment for the data bytes of a meta-event.</summary>
        /// <param name="type">Meta-event type (always less than 128).</param>
        /// <param name="bytes">The data bytes of the event.</param>
        /// <returns>A comment for the data bytes.</returns>
        public static string GetDataComment(int type, byte[] bytes)
        {
            return (MidiMetaEvent.TypeMap.ContainsKey(type) && MidiMetaEvent.TypeMap[type].DescribeData != null)
                ? MidiMetaEvent.TypeMap[type].DescribeData(bytes) : Midi.ReadText(bytes, 0, bytes.Length);
        }

        /// <summary>Attempts to convert the data bytes of a meta-event to a MidiKeySignature value.</summary>
        /// <param name="bytes">The data bytes of the event.</param>
        /// <returns>
        /// The data bytes converted to a MidiKeySignature value, or MidiKeySignature.NA if the data could not be converted.
        /// </returns>
        public static MidiKeySignature DataToKeySignature(byte[] bytes)
        {
            int n = Midi.ReadNumber(bytes, 0, bytes.Length);
            return Enum.IsDefined(typeof(MidiKeySignature), n) ? (MidiKeySignature)n : MidiKeySignature.NA;
        }

        #endregion

        #region Private Methods

        private static string DescribeNumber(byte[] bytes) { return Midi.ReadNumber(bytes, 0, bytes.Length).ToString(); }

        private static string DescribeTempo(byte[] bytes)
        {
            int n = Midi.ReadNumber(bytes, 0, bytes.Length);
            return string.Format("{0} {1} ({2} {3})", n, Properties.Resources.MicrosecondsPerQuarterNote,
                (n == 0) ? double.PositiveInfinity : (60000000 / n), Properties.Resources.BeatsPerMinute);
        }

        private static string DescribeSmpteOffset(byte[] bytes)
        {
            string s = string.Empty;
            foreach (byte b in bytes)
            {
                if (s.Length > 0) s += (s.Length > 9) ? "." : ":";
                s += b.ToString("00");
            }
            return s;
        }

        private static string DescribeTimeSignature(byte[] bytes)
        {
            return string.Format("{0}/{1} {2}, {3} {4}, {5} {6}", bytes[0], Math.Pow(2, bytes[1]), Properties.Resources.Time,
                bytes[2], Properties.Resources.ClocksPerClick, bytes[3], Properties.Resources.NotesPerQuarterNote);
        }

        private static string DescribeKeySignature(byte[] bytes)
        {
            MidiKeySignature k = MidiMetaEvent.DataToKeySignature(bytes);
            return (k == MidiKeySignature.NA) ? null :
                k.ToString().Replace("Flat", "b").Replace("Sharp", "#").Replace("Major", " major").Replace("Minor", " minor");
        }

        #endregion
    }
}
