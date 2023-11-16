using System;

namespace SecureFolderFS.Shared.Extensions
{
    public static class SpanExtensions
    {
        public static bool IsAllZeros(this ReadOnlySpan<byte> span)
        {
            var all = true;
            for (var i = 0; i < span.Length; i++)
            {
                all &= span[i] == 0;
                if (!all)
                    break;
            }

            return all;
        }
    }
}
