/* MidiSysExEvent.cs - Implementation of MidiSysExEvent class, which corresponds
 * to a system exclusive (SysEx) message/event in the MIDI file spec.
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
    /// <summary>Corresponds to a system exclusive (SysEx) message/event in the MIDI file spec.</summary>
    public sealed class MidiSysExEvent : MidiEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiSysExEvent class.</summary>
        /// <param name="file">MidiFile object representing the MIDI file to which this item belongs.</param>
        /// <param name="offset">Offset into the file's byte array at which this item begins.</param>
        public MidiSysExEvent(MidiFile file, int offset) : base(file, offset) { }

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes represented by this item.</summary>
        public override int Size { get { return MidiSysExEvent.TotalSize(this.DeltaTime, this.DataLength); } }

        /// <summary>Gets the text to display in the "Type" field.</summary>
        public override string TypeField
        {
            get
            {
                return Properties.Resources.SysEx + " " +
                    UI.ParseLabel(this.Escape ? Properties.Resources.Escape : Properties.Resources.Message).ToLower();
            }
        }

        /// <summary>Gets the text to display in the "Count" field (i.e., data length).</summary>
        public override string CountField { get { return MidiItem.FormatCount(this.DataLength, Properties.Resources.Byte); } }

        /// <summary>Gets the text to display in the "Data" field.</summary>
        public override string DataField { get { return this.File.ReadText(this.DataOffset, this.DataLength); } }

        /// <summary>
        /// False indicates a regular SysEx message, which could be the first in a series of timed packets.
        /// True indicates a SysEx "escape," or the next in a series of timed packets.
        /// </summary>
        public bool Escape
        {
            get { return this.File.ReadNumber(this.StatusOffset, 1) == 0xF7; }
            set { this.File.WriteNumber(this.StatusOffset, (value ? 0xF7 : 0xF0), 1); }
        }

        /// <summary>The data bytes of the event.</summary>
        public byte[] Data
        {
            get { return this.File.ReadBytes(this.DataOffset, this.DataLength); }
            set
            {
                this.File.WriteBytes(this.DataOffset, value, this.DataLength);
                this.DataLength = (value == null) ? 0 : value.Length;
            }
        }

        #endregion

        #region Private Properties

        /// <summary>Number of bytes which follow the data length (not including the data length itself).</summary>
        private int DataLength
        {
            get { return this.File.ReadVLQ(this.DataLengthOffset); }
            set
            {
                int n = Midi.SizeVLQ(this.DataLength);
                this.File.WriteVLQ(this.DataLengthOffset, value, n);
            }
        }

        private int DataLengthOffset { get { return this.StatusOffset + 1; } }
        private int DataOffset { get { return this.DataLengthOffset + Midi.SizeVLQ(this.DataLength); } }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Returns the total number of bytes required to store an item of this type.</summary>
        public static int TotalSize(int deltaTime, int dataLength)
        { return Midi.SizeVLQ(deltaTime) + 1 + Midi.SizeVLQ(dataLength) + dataLength; }

        #endregion
    }
}
