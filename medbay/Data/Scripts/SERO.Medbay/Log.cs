// <copyright file="Gasses.cs" company="Cult of Clang">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>Lize Live</author>

using System;
using Sandbox.ModAPI;
using VRage.Utils;

namespace SERO
{
    public static class Log
    {
        public static void Error(Exception exception)
        {
            Line($"iamerror {exception}");
        }
        public static void Line(string v)
        {
            MyLog.Default.WriteLine(v);
            MyAPIGateway.Utilities.ShowMessage("LOG", v);
        }
    }
}