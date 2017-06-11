using System;
using System.Reflection;

namespace IssueTracker.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Extracts the timestamp when it was built from the assembly.
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        public static DateTime GetLinkerTimestamp(this Assembly asm)
        {
            string filePath = asm.Location;

            const int cPeHeaderOffset = 60;
            const int cLinkerTimestampOffset = 8;
            var b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                s?.Close();
            }

            int i = BitConverter.ToInt32(b, cPeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + cLinkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);

            // offset it by the current timezone, since timestamp is always UTC
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }
    }
}