using System.Collections.Generic;
using System.Text;

namespace RemoteConfiguration;

public static class StringExtensions
{
    public static string ToHex(this IEnumerable<byte> bytes)
    {
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append($"{b:x2}");
        }

        return builder.ToString();
    }
}