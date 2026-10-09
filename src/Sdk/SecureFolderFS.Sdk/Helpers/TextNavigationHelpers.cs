using System;

namespace SecureFolderFS.Sdk.Helpers
{
    /// <summary>
    /// Finds the positions that caret movement commands lead to in plain text, where text controls do not provide them.
    /// </summary>
    /// <remarks>
    /// Both '\r' and '\n' count as line breaks, as text boxes report line breaks as '\r'.
    /// </remarks>
    public static class TextNavigationHelpers
    {
        /// <summary>
        /// Gets where the caret moves when going up by paragraph: to the start of its paragraph or, when it is
        /// there already, to the start of the paragraph above (as Option+Up does on macOS).
        /// </summary>
        /// <param name="text">The text being edited.</param>
        /// <param name="index">The position of the caret.</param>
        /// <returns>The position to move the caret to.</returns>
        public static int GetParagraphStart(string text, int index)
        {
            index = Math.Clamp(index, 0, text.Length);
            if (index > 0 && IsLineBreak(text[index - 1]))
                index--;

            return GetLineStart(text, index);
        }

        /// <summary>
        /// Gets where the caret moves when going down by paragraph: to the end of its paragraph or, when it is
        /// there already, to the end of the paragraph below (as Option+Down does on macOS).
        /// </summary>
        /// <param name="text">The text being edited.</param>
        /// <param name="index">The position of the caret.</param>
        /// <returns>The position to move the caret to.</returns>
        public static int GetParagraphEnd(string text, int index)
        {
            index = Math.Clamp(index, 0, text.Length);
            if (index < text.Length && IsLineBreak(text[index]))
                index++;

            return GetLineEnd(text, index);
        }

        /// <summary>
        /// Gets the start of the line the caret is on, delimited by line breaks rather than by wrapping.
        /// </summary>
        /// <param name="text">The text being edited.</param>
        /// <param name="index">The position of the caret.</param>
        /// <returns>The position right after the preceding line break, or 0.</returns>
        public static int GetLineStart(string text, int index)
        {
            index = Math.Clamp(index, 0, text.Length);
            while (index > 0 && !IsLineBreak(text[index - 1]))
                index--;

            return index;
        }

        /// <summary>
        /// Gets the end of the line the caret is on, delimited by line breaks rather than by wrapping.
        /// </summary>
        /// <param name="text">The text being edited.</param>
        /// <param name="index">The position of the caret.</param>
        /// <returns>The position of the following line break, or the length of the text.</returns>
        public static int GetLineEnd(string text, int index)
        {
            index = Math.Clamp(index, 0, text.Length);
            while (index < text.Length && !IsLineBreak(text[index]))
                index++;

            return index;
        }

        private static bool IsLineBreak(char c)
        {
            return c is '\r' or '\n';
        }
    }
}
