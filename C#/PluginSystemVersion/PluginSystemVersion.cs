﻿/*
  Copyright (C) 2012 Birunthan Mohanathas

  This program is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rainmeter;

// Overview: This example demonstrates the basic concept of Rainmeter C# plugins.

// Sample skin:
/*
    [Rainmeter]
    Update=1000
    BackgroundMode=2
    SolidColor=000000

    [mString]
    Measure=Plugin
    Plugin=SystemVersion.dll
    Type=String

    [mMajor]
    Measure=Plugin
    Plugin=SystemVersion.dll
    Type=Major

    [mMinor]
    Measure=Plugin
    Plugin=SystemVersion.dll
    Type=Minor

    [mNumber]
    Measure=Plugin
    Plugin=SystemVersion.dll
    Type=Number

    [Text1]
    Meter=STRING
    MeasureName=mString
    MeasureName2=mMajor
    MeasureName3=mMinor
    MeasureName4=mNumber
    X=5
    Y=5
    W=300
    H=70
    FontColor=FFFFFF
    Text="String: %1#CRLF#Major: %2#CRLF#Minor: %3#CRLF#Number: %4#CRLF#"

    [Text2]
    Meter=STRING
    MeasureName=mString
    MeasureName2=mMajor
    MeasureName3=mMinor
    MeasureName4=mNumber
    NumOfDecimals=1
    X=5
    Y=5R
    W=300
    H=70
    FontColor=FFFFFF
    Text="String: %1#CRLF#Major: %2#CRLF#Minor: %3#CRLF#Number: %4#CRLF#"
*/

namespace PluginSystemVersion
{
    internal class Measure
    {
        enum MeasureType
        {
            Major,
            Minor,
            Number,
            String
        }

        MeasureType Type = MeasureType.Major;

        internal Measure()
        {
        }

        internal void Reload(Rainmeter.API rm, ref double maxValue)
        {
            string type = rm.ReadString("Type", "");
            switch (type.ToLowerInvariant())
            {
                case "major":
                    Type = MeasureType.Major;
                    break;

                case "minor":
                    Type = MeasureType.Minor;
                    break;

                case "number":
                    Type = MeasureType.Number;
                    break;

                case "string":
                    Type = MeasureType.String;
                    break;

                default:
                    API.Log(API.LogType.Error, "SystemVersion.dll: Type=" + type + " not valid");
                    break;
            }
        }

        internal double Update()
        {
            switch (Type)
            {
                case MeasureType.Major:
                    return (double)Environment.OSVersion.Version.Major;

                case MeasureType.Minor:
                    return (double)Environment.OSVersion.Version.Minor;

                case MeasureType.Number:
                    return (double)Environment.OSVersion.Version.Major + ((double)Environment.OSVersion.Version.Minor / 10.0);
            }

            // MeasureType.MajorMinor is not a number and and therefore will be
            // returned in GetString.

            return 0.0;
        }

        internal string GetString()
        {
            switch (Type)
            {
                case MeasureType.String:
                    return string.Format("{0}.{1} (Build {2})", Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor, Environment.OSVersion.Version.Build);
            }

            // MeasureType.Major, MeasureType.Minor, and MeasureType.Number are
            // numbers. Therefore, null is returned here for them. This is to
            // inform Rainmeter that it can treat those types as numbers.

            return null;
        }
    }

    public static class Plugin
    {
        internal static Dictionary<uint, Measure> Measures = new Dictionary<uint, Measure>();

        [DllExport]
        public unsafe static void Initialize(void** data, void* rm)
        {
            uint id = (uint)((void*)*data);
            Measures.Add(id, new Measure());
        }

        [DllExport]
        public unsafe static void Finalize(void* data)
        {
            uint id = (uint)data;
            Measures.Remove(id);
        }

        [DllExport]
        public unsafe static void Reload(void* data, void* rm, double* maxValue)
        {
            uint id = (uint)data;
            Measures[id].Reload(new Rainmeter.API((IntPtr)rm), ref *maxValue);
        }

        [DllExport]
        public unsafe static double Update(void* data)
        {
            uint id = (uint)data;
            return Measures[id].Update();
        }

        [DllExport]
        public unsafe static char* GetString(void* data)
        {
            uint id = (uint)data;
            fixed (char* s = Measures[id].GetString()) return s;
        }
    }
}
