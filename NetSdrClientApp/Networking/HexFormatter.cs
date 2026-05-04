using System;
using System.Linq;

namespace NetSdrClientApp.Networking
{
    internal static class HexFormatter
    {
        public static string ToSpaceSeparatedHex(byte[] data)
        {
            if (data is null || data.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(" ", data.Select(b => Convert.ToString(b, toBase: 16)));
        }
    }
}
