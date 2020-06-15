/* MidiItem.cs - Implementation of MidiItem class, which represents one line of data (e.g., an event) from a MIDI file.
 *
 * Copyright (c) 2018-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* Thickness */
using System.Windows;


namespace JeffBourdier
{
    /// <summary>Represents one line of data (e.g., an event) from a MIDI file.</summary>
    public abstract class MidiItem
    {
        /*********
         * Types *
         *********/

        #region Public Types

        /// <summary>Control that is used to separate MidiItem objects in an item control (e.g., a list view).</summary>
        /// <remarks>
        /// This is to prevent System.Windows.Data Error 39: BindingExpression path error (property
        /// not found) when adding a separator to the grid/list view (see MIDIopsyWindow.AddViewItem).
        /// </remarks>
        public class Separator : System.Windows.Controls.Separator
        {
            public string HexField { get { return null; } }
            public string TotalField { get { return null; } }
            public string DeltaField { get { return null; } }
            public string TypeField { get { return null; } }
            public string CountField { get { return null; } }
            public string DataField { get { return null; } }
        }

        /// <summary>Defines how MidiItem objects are displayed in "fields" (grid view columns/cells, etc.).</summary>
        public struct DisplayField
        {
            public DisplayField(string bindingPath, string labelText, Thickness borderThickness,
                bool monospace, bool rightAlign, double widthFactor)
            {
                this._BindingPath = bindingPath;
                this._LabelText = labelText;
                this._BorderThickness = borderThickness;
                this._Monospace = monospace;
                this._RightAlign = rightAlign;
                this._WidthFactor = widthFactor;
            }

            private string _BindingPath;
            private string _LabelText;
            private Thickness _BorderThickness;
            private bool _Monospace;
            private bool _RightAlign;
            private double _WidthFactor;

            public string BindingPath { get { return this._BindingPath; } }
            public string LabelText { get { return this._LabelText; } }
            public Thickness BorderThickness { get { return this._BorderThickness; } }
            public bool Monospace { get { return this._Monospace; } }
            public bool RightAlign { get { return this._RightAlign; } }
            public double WidthFactor { get { return this._WidthFactor; } }
        }

        #endregion

        /****************
         * Constructors *
         ****************/

        #region Public Constructors

        /// <summary>Initializes a new instance of the MidiItem class.</summary>
        /// <param name="file">MidiFile object representing the MIDI file to which this item belongs.</param>
        /// <param name="offset">Offset into the file's byte array at which this item begins.</param>
        public MidiItem(MidiFile file, int offset) { this._File = file; this.Offset = offset; }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        /* The border thicknesses must be initialized before the display fields. */
        private static readonly Thickness InitialBorderThickness = new Thickness(1, 2, UI.DoubleSpace, 2);
        private static readonly Thickness DefaultBorderThickness = new Thickness(1);

        private MidiFile _File;

        #endregion

        #region Public Fields

        /// <summary>Array of DisplayField objects defining standard fields for displaying MidiItem objects.</summary>
        public static readonly DisplayField[] DisplayFields = new DisplayField[]
        {
            new DisplayField("HexField", Properties.Resources.Hex, MidiItem.InitialBorderThickness, true, false, 0.2),
            new DisplayField("TotalField", Properties.Resources.TotalTime, MidiItem.DefaultBorderThickness, false, true, 0.1),
            new DisplayField("DeltaField", Properties.Resources.DeltaTime, MidiItem.DefaultBorderThickness, false, true, 0.1),
            new DisplayField("TypeField", Properties.Resources.Type, MidiItem.DefaultBorderThickness, false, false, 0.2),
            new DisplayField("CountField", Properties.Resources.Count, MidiItem.DefaultBorderThickness, false, true, 0.1),
            new DisplayField("DataField", Properties.Resources.Data, MidiItem.DefaultBorderThickness, false, false, 0.3)
        };

        /// <summary>Offset into the file's byte array at which this item begins.</summary>
        public int Offset;

        #endregion

        /**************
         * Properties *
         **************/

        #region Public Properties

        /// <summary>Gets the number of bytes represented by this item.</summary>
        public virtual int Size { get { return 0; } }

        /// <summary>
        /// Gets the text to display in the "Hex" field (i.e., the bytes represented by this item, in hexadecimal format).
        /// </summary>
        public virtual string HexField { get { return this.File.FormatHex(this.Offset, this.Size); } }

        /// <summary>Gets the text to display in the "Total" field.</summary>
        /// <remarks>For MIDI events, this should be total (cumulative) time.</remarks>
        public virtual string TotalField { get { return null; } }

        /// <summary>Gets the text to display in the "Delta" field.</summary>
        /// <remarks>For MIDI events, this should be delta-time.</remarks>
        public virtual string DeltaField { get { return null; } }

        /// <summary>Gets the text to display in the "Type" field (file format, chunk/event type, etc.).</summary>
        public virtual string TypeField { get { return null; } }

        /// <summary>Gets the text to display in the "Count" field (e.g., data length or MIDI channel).</summary>
        public virtual string CountField { get { return null; } }

        /// <summary>Gets the text to display in the "Data" field.</summary>
        public virtual string DataField { get { return null; } }

        #endregion

        #region Protected Properties

        /// <summary>Gets the MidiFile object representing the MIDI file to which this item belongs.</summary>
        protected MidiFile File { get { return this._File; } }

        #endregion
    }
}
