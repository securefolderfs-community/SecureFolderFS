using System;
using System.Collections.Generic;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseCountToStringConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not int count)
                return null;

            if (parameter is not string strParam)
                return count.ToString();

            // Example: for 0 items, use SelectItems localized string,
            // for more than 0 items, use ItemsSelected localized string
            //
            // (0:LOCALIZE|SelectItems)|(>0:LOCALIZE|ItemsSelected)
            // STANDARD - don't localize, use string directly

            // Parse each clause: (condition:mode|value)
            foreach (var clause in ParseClauses(strParam))
            {
                if (!MatchesCondition(clause.Condition, count))
                    continue;

                return clause.Mode.Equals("LOCALIZE", StringComparison.OrdinalIgnoreCase)
                    ? clause.Text.ToLocalized(count)
                    : clause.Text;
            }

            return count.ToString();
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }

        private static bool MatchesCondition(string condition, int count)
        {
            // Supported: exact number (e.g. "0"), ">N", ">=N", "<N", "<=N"
            if (condition.StartsWith(">="))
                return int.TryParse(condition[2..], out var n) && count >= n;

            if (condition.StartsWith(">"))
                return int.TryParse(condition[1..], out var n) && count > n;

            if (condition.StartsWith("<="))
                return int.TryParse(condition[2..], out var n) && count <= n;

            if (condition.StartsWith("<"))
                return int.TryParse(condition[1..], out var n) && count < n;

            if (int.TryParse(condition, out var exact))
                return count == exact;

            return false;
        }

        private static IEnumerable<(string Condition, string Mode, string Text)> ParseClauses(string input)
        {
            // Each clause is wrapped in parentheses: (condition:mode|text)
            // Clauses are separated by |, but only outside of parentheses
            var i = 0;
            while (i < input.Length)
            {
                if (input[i] != '(')
                {
                    i++;
                    continue;
                }

                var closing = input.IndexOf(')', i);
                if (closing < 0)
                    break;

                // Extract the content inside the parentheses: "condition:mode|text"
                var content = input[(i + 1)..closing];

                var colonIndex = content.IndexOf(':');
                var pipeIndex = content.IndexOf('|');

                if (colonIndex >= 0 && pipeIndex > colonIndex)
                {
                    var condition = content[..colonIndex].Trim();
                    var mode = content[(colonIndex + 1)..pipeIndex].Trim();
                    var text = content[(pipeIndex + 1)..].Trim();
                    yield return (condition, mode, text);
                }

                i = closing + 1;
            }
        }
    }
}
