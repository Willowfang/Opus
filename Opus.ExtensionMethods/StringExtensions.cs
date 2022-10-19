using Opus.Values;

namespace Opus.ExtensionMethods
{
    /// <summary>
    /// Enum describing the type of a placeholder string (e.g. "[bookmark]").
    /// </summary>
    public enum Placeholders
    {
        /// <summary>
        /// Placeholder for bookmark name.
        /// </summary>
        Bookmark,
        /// <summary>
        /// Placeholder for file name.
        /// </summary>
        File,
        /// <summary>
        /// Placeholder for automated numbering.
        /// </summary>
        Number
    }

    /// <summary>
    /// Common extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replace a placeholder string in a name template.
        /// </summary>
        /// <param name="template">Template string to apply this action on.</param>
        /// <param name="placeholder">Placeholder to replace.</param>
        /// <param name="replacement">What to replace the placeholder with.</param>
        /// <returns>String with replacements.</returns>
        public static string ReplacePlaceholder(
            this string template,
            Placeholders placeholder,
            string replacement
        )
        {
            string placeholderName = placeholder.ToString();

            // Get correct cultural string representation of the placeholder.

            foreach (string code in SupportedTypes.CULTURES)
            {
                string? placeholderTemplate =
                    Resources.Placeholders.FileNames.ResourceManager.GetString(
                        placeholderName,
                        new System.Globalization.CultureInfo(code)
                    );

                if (placeholderTemplate != null)
                    template = template.Replace(placeholderTemplate, replacement);
            }

            return template;
        }

        /// <summary>
        /// Replace characters not allowed by the filesystem with allowed characters.
        /// </summary>
        /// <param name="original">String to replace characters in.</param>
        /// <returns>A new, legal string.</returns>
        public static string ReplaceIllegal(this string original)
        {
            string processed = original.Replace(":", "");
            processed = processed.Replace("/", "-");
            return string.Join("", processed.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
