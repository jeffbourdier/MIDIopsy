/* Meta.cs - Implementation of Meta class, which provides metadata pertaining to this assembly.
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
    /// <summary>Provides metadata pertaining to this assembly.</summary>
    internal static class Meta
    {
        public const string Name = "MIDIopsy";
        public const string FullName = Meta.Name + " by " + AppHelper.AuthorName;
        public const string Version = "1.1";
    }
}
