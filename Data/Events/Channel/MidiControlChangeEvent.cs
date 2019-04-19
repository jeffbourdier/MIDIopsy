/* MidiControlChangeEvent.cs - Implementation of MidiControlChangeEvent class, which
 * corresponds to a "Control Change" channel message/event in the MIDI file spec.
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
    /// <summary>Corresponds to a "Control Change" channel message/event in the MIDI file spec.</summary>
    public class MidiControlChangeEvent : MidiChannelEvent
    {
        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiControlChangeEvent class.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="channel">One of the sixteen logical MIDI channels on which this event is transmitted.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        /// <remarks>To create an event with running status, use the other constructor.</remarks>
        public MidiControlChangeEvent(int deltaTime, uint channel, byte[] bytes, int index)
            : base(deltaTime, 0xB, channel, 3, Properties.Resources.ControlChange) { this.Initialize(bytes, index); }

        /// <summary>Initializes a new instance of the MidiControlChangeEvent class using running status.</summary>
        /// <param name="deltaTime">The amount of time (in ticks) between the previous event in the track and this one.</param>
        /// <param name="bytes">Array of bytes containing the event data (not including the delta-time or status byte).</param>
        /// <param name="index">Index in the byte array at which the event data begins.</param>
        public MidiControlChangeEvent(int deltaTime, byte[] bytes, int index)
            : base(deltaTime, 2) { this.Initialize(bytes, index); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private uint _ControllerNumber;
        private string ControlFunction;
        private uint _ControlValue;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Controller number.</summary>
        public uint ControllerNumber
        {
            get { return this._ControllerNumber; }

            set
            {
                MidiData.ValidateNumber(value, 0, 119, Properties.Resources.ControllerNumber);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset);
                this._ControllerNumber = value;
                this.ControlFunction = MidiControlChangeEvent.GetControlFunction(value);
                this.SetDataComment();
            }
        }

        /// <summary>Control value.</summary>
        public uint ControlValue
        {
            get { return this._ControlValue; }

            set
            {
                MidiData.ValidateNumber(value, 0, (uint)sbyte.MaxValue, Properties.Resources.ControlValue);
                MidiData.WriteNumber(value, 1, this.Bytes, this.DeltaTimeSize + this.DataOffset + 1);
                this._ControlValue = value;
                this.SetDataComment();
            }
        }

        #endregion

        /***********
         * Methods *
         ***********/

        #region Private Methods

        private void Initialize(byte[] bytes, int index)
        {
            this.ControllerNumber = MidiData.ReadNumber(bytes, 1, index);
            this.ControlValue = MidiData.ReadNumber(bytes, 1, index + 1);
        }

        private static string GetControlFunction(uint n)
        {
            switch (n)
            {
                case 0: return Properties.Resources.Controller0;
                case 1: return Properties.Resources.Controller1;
                case 2: return Properties.Resources.Controller2;
                case 4: return Properties.Resources.Controller4;
                case 5: return Properties.Resources.Controller5;
                case 6: return Properties.Resources.Controller6;
                case 7: return Properties.Resources.Controller7;
                case 8: return Properties.Resources.Controller8;
                case 10: return Properties.Resources.Controller10;
                case 11: return Properties.Resources.Controller11;
                case 12: return Properties.Resources.Controller12;
                case 13: return Properties.Resources.Controller13;
                case 16: return Properties.Resources.Controller16;
                case 17: return Properties.Resources.Controller17;
                case 18: return Properties.Resources.Controller18;
                case 19: return Properties.Resources.Controller19;
                case 32: return Properties.Resources.Controller32;
                case 33: return Properties.Resources.Controller33;
                case 34: return Properties.Resources.Controller34;
                case 35: return Properties.Resources.Controller35;
                case 36: return Properties.Resources.Controller36;
                case 37: return Properties.Resources.Controller37;
                case 38: return Properties.Resources.Controller38;
                case 39: return Properties.Resources.Controller39;
                case 40: return Properties.Resources.Controller40;
                case 41: return Properties.Resources.Controller41;
                case 42: return Properties.Resources.Controller42;
                case 43: return Properties.Resources.Controller43;
                case 44: return Properties.Resources.Controller44;
                case 45: return Properties.Resources.Controller45;
                case 46: return Properties.Resources.Controller46;
                case 47: return Properties.Resources.Controller47;
                case 48: return Properties.Resources.Controller48;
                case 49: return Properties.Resources.Controller49;
                case 50: return Properties.Resources.Controller50;
                case 51: return Properties.Resources.Controller51;
                case 52: return Properties.Resources.Controller52;
                case 53: return Properties.Resources.Controller53;
                case 54: return Properties.Resources.Controller54;
                case 55: return Properties.Resources.Controller55;
                case 56: return Properties.Resources.Controller56;
                case 57: return Properties.Resources.Controller57;
                case 58: return Properties.Resources.Controller58;
                case 59: return Properties.Resources.Controller59;
                case 60: return Properties.Resources.Controller60;
                case 61: return Properties.Resources.Controller61;
                case 62: return Properties.Resources.Controller62;
                case 63: return Properties.Resources.Controller63;
                case 64: return Properties.Resources.Controller64;
                case 65: return Properties.Resources.Controller65;
                case 66: return Properties.Resources.Controller66;
                case 67: return Properties.Resources.Controller67;
                case 68: return Properties.Resources.Controller68;
                case 69: return Properties.Resources.Controller69;
                case 70: return Properties.Resources.Controller70;
                case 71: return Properties.Resources.Controller71;
                case 72: return Properties.Resources.Controller72;
                case 73: return Properties.Resources.Controller73;
                case 74: return Properties.Resources.Controller74;
                case 75: return Properties.Resources.Controller75;
                case 76: return Properties.Resources.Controller76;
                case 77: return Properties.Resources.Controller77;
                case 78: return Properties.Resources.Controller78;
                case 79: return Properties.Resources.Controller79;
                case 80: return Properties.Resources.Controller80;
                case 81: return Properties.Resources.Controller81;
                case 82: return Properties.Resources.Controller82;
                case 83: return Properties.Resources.Controller83;
                case 84: return Properties.Resources.Controller84;
                case 91: return Properties.Resources.Controller91;
                case 92: return Properties.Resources.Controller92;
                case 93: return Properties.Resources.Controller93;
                case 94: return Properties.Resources.Controller94;
                case 95: return Properties.Resources.Controller95;
                case 96: return Properties.Resources.Controller96;
                case 97: return Properties.Resources.Controller97;
                case 98: return Properties.Resources.Controller98;
                case 99: return Properties.Resources.Controller99;
                case 100: return Properties.Resources.Controller100;
                case 101: return Properties.Resources.Controller101;
                default: return Properties.Resources.Undefined;
            }
        }

        private void SetDataComment()
        {
            this.DataComment = string.Format("{0} {1} ({2}), {3} {4}",
                Properties.Resources.ControllerNumber, this.ControllerNumber, this.ControlFunction,
                Properties.Resources.ControlValue.ToLower(), this.ControlValue);
            this.SetComment();
        }

        #endregion
    }
}
