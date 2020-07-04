/* Logger.cs - Implementation of Logger class, which writes messages to the log file specified in the user's config file.
 * Note that this file is shared across applications.
 *
 * Copyright (c) 2017-20 Jeffrey Paul Bourdier
 *
 * Licensed under the MIT License.  This file may be used only in compliance with this License.
 * Software distributed under this License is provided "AS IS", WITHOUT WARRANTY OF ANY KIND.
 * For more information, see the accompanying License file or the following URL:
 *
 *   https://opensource.org/licenses/MIT
 */


/* DateTime, Environment, Exception, StringSplitOptions */
using System;

/* StreamWriter */
using System.IO;

/* MessageBox, MessageBoxButton, MessageBoxImage, Window */
using System.Windows;


namespace JeffBourdier
{
    /// <summary>Writes messages to the log file specified in the user's config file.</summary>
    public static class Logger
    {
        /****************
         * Constructors *
         ****************/

        #region Private Constructors

        /// <summary>
        /// Initializes the logger object.  This constructor is called upon the first call to any member of this class.
        /// </summary>
        static Logger() { Logger.ReadSettings(); }

        #endregion

        /**********
         * Fields *
         **********/

        #region Private Fields

        private static bool Enabled;
        private static string FilePath;
        private static bool IncludeTimestamp;
        private static bool IncludeProcedureName;
        private static bool Indent;
        private static bool IncludeExceptionDetail;

        #endregion

        /***********
         * Methods *
         ***********/

        #region Public Methods

        /// <summary>Read log settings from the config file.</summary>
        public static void ReadSettings()
        {
            Logger.Enabled = Common.Settings.Default.Log;
            Logger.FilePath = Common.Settings.Default.LogPath;
            Logger.IncludeTimestamp = Common.Settings.Default.LogTimestamp;
            Logger.IncludeProcedureName = Common.Settings.Default.LogProcedureName;
            Logger.Indent = Common.Settings.Default.LogIndent;
            Logger.IncludeExceptionDetail = Common.Settings.Default.LogExceptionDetail;

            /* Test the log file first to make sure it can be written to. */
            try { using (StreamWriter writer = new StreamWriter(Logger.FilePath, true)) { } }
            catch { Logger.Enabled = false; }
        }

        /// <summary>Writes a message to the log file.</summary>
        /// <param name="message">The message to write.</param>
        public static void WriteMessage(string message)
        {
            /* If we're not logging, return. */
            if (!Logger.Enabled) return;

            /* We're logging, so format and write the message. */
            int i;
            string s = null;
            if (Logger.IncludeTimestamp) s += DateTime.Now + " ";
            switch (Logger.GetProcedureName(1, false))
            {
                case "Logger.WriteProcedureMessage": i = 3; break;
                case "Logger.WriteException": i = 2; break;
                default: i = 1; break;
            }
            if (Logger.IncludeProcedureName) s += Logger.GetProcedureName(i, Logger.Indent) + ": ";
            s += message;
            Logger.Write(s);
        }

        /// <summary>Writes a message to the log file indicating that a procedure is being entered.</summary>
        public static void WriteEntrance() { Logger.WriteProcedureMessage(Common.Resources.Entering); }

        /// <summary>Writes a message to the log file indicating that a procedure is being exited.</summary>
        public static void WriteExit() { Logger.WriteProcedureMessage(Common.Resources.Exiting); }

        /// <summary>Writes information about an exception to the log file.</summary>
        /// <param name="exception">The exception to log.</param>
        public static void WriteException(Exception exception)
        {
            /* If we're not logging, return. */
            if (!Logger.Enabled) return;

            /* We're logging, so write the message.  If no detail, write only the Message property and return. */
            if (!Logger.IncludeExceptionDetail)
            {
                Logger.WriteMessage(exception.Message);
                return;
            }

            /* We're logging detail, so write the results of ToString. */
            string s = exception.ToString();
            string[] lines = new string[] { Environment.NewLine };
            lines = s.Split(lines, StringSplitOptions.None);
            foreach (string line in lines) Logger.WriteMessage(line);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Writes to the log file a message about a procedure using the
        /// specified string (typically to indicate entrance or exit).
        /// </summary>
        private static void WriteProcedureMessage(string s)
        {
            /* Format the message depending on whether or not we're logging procedure names. */
            if (Logger.IncludeProcedureName) s = string.Format("<{0}>", s);
            else s += " " + Logger.GetProcedureName(2, false);
            Logger.WriteMessage(s);
        }

        /// <summary>Writes a string followed by a line terminator to (and then closes) the log file.</summary>
        /// <param name="value">The string to write.  If null, only the line termination characters are written.</param>
        private static void Write(string value)
        {
            using (StreamWriter writer = new StreamWriter(Logger.FilePath, true))
                lock (writer)
                {
                    writer.WriteLine(value);
                    writer.Close();
                }
        }

        /// <summary>Returns the name of the calling procedure (or some procedure above it in the call stack).</summary>
        /// <param name="level">
        /// The number of levels in the call stack above the calling procedure to look for the return value.
        /// To return the name of the calling procedure, set this argument to 0; to return the
        /// name of the procedure that called the calling procedure, set it to 1; and so on.
        /// </param>
        /// <param name="indent">
        /// True to prefix the return value with two dots (..) for each
        /// call stack level beyond the entry point; otherwise, false.
        /// </param>
        /// <returns>The name of the specified procedure in the call stack.</returns>
        public static string GetProcedureName(int level, bool indent)
        {
            const string token = "   at ";

            /* The first 3 calls are to the following procedures (in this order), none of which we're interested in:
             *   - Environment.GetStackTrace
             *   - Environment.get_StackTrace
             *   - Logger.GetProcedureName (this method)
             * The fourth is the procedure that called this method.  This is the first
             * procedure in which we might be interested, so get its starting index.
             */
            int i = 0;
            for (int j = 0; j < 4; ++j) i = Environment.StackTrace.IndexOf(token, i) + token.Length;

            /* Do we want the name of the procedure that called this method, or the
             * name of the procedure that called the procedure that called this method?
             */
            for (int j = 0; j < level; ++j)
            {
                /* We want the name of the procedure that called the procedure that called this method, so do one
                 * more iteration.  Note the return value though, because there may not be a procedure at that level.
                 */
                i = Environment.StackTrace.IndexOf(token, i);
                if (i < 0) return null;
                i += token.Length;
            }

            /* Get the full procedure name. */
            int n = Environment.StackTrace.IndexOf("(", i) - i;
            string s = Environment.StackTrace.Substring(i, n);

            /* This full procedure name includes namespace, which we don't want,
             * so strip that off, leaving just class name and procedure name.
             */
            for (n = s.LastIndexOf("."); s[n] == '.'; --n) ;
            n = s.LastIndexOf(".", n) + 1;
            s = s.Substring(n);

            /* If indenting, determine the call stack level. */
            if (indent) while ((i = Environment.StackTrace.IndexOf(Environment.NewLine, ++i)) > 0) s = ".." + s;
            return s;
        }

        #endregion
    }
}
