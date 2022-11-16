using Opus.Common.Services.Base;
using Opus.Resources.Buttons.Extraction;
using Opus.Values;
using System.IO;
using WF.LoggingLib;

namespace Opus.Common.Extensions
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
        /// Replace a placeholder name string in a name template.
        /// </summary>
        /// <param name="template">Template string to apply this action on.</param>
        /// <param name="placeholder">Placeholder to replace.</param>
        /// <param name="replacement">What to replace the placeholder with.</param>
        /// <param name="logbook">Logging service.</param>
        /// <returns>String with replacements.</returns>
        public static string ReplacePlaceholder(
            this string template,
            Placeholders placeholder,
            string replacement,
            ILogbook? logbook = null)
        {
            string placeholderName = placeholder.ToString();

            // Get correct cultural string representation of the placeholder.

            string? placeholderTemplate = null;

            foreach (string code in SupportedTypes.CULTURES)
            {
                placeholderTemplate =
                    Resources.Placeholders.FileNames.ResourceManager.GetString(
                        placeholderName,
                        new System.Globalization.CultureInfo(code)
                    );

                if (string.IsNullOrEmpty(placeholderTemplate))
                {
                    continue;
                }
                else
                {
                    template = template.Replace(placeholderTemplate, replacement);
                }
            }

            if (placeholderTemplate != null)
                logbook?.Write($"Replaced {placeholderTemplate} with {replacement}.", LogLevel.Debug);
            else
                logbook?.Write($"No replacable placeholder found.", LogLevel.Debug);

            return template;
        }

        /// <summary>
        /// Replace the number placeholder of a string with the index value of a given item.
        /// <para>
        /// If needed and if an enumerable of all items is provided, additional zeroes will be added to 
        /// even out the number of numbers (01, 02 .. 001, 002.. etc).
        /// </para>
        /// </summary>
        /// <typeparam name="T">Type of the items. They must be indexed.</typeparam>
        /// <param name="template">Template where the placeholder will be replaced.</param>
        /// <param name="items">All items in the enumerable. If null, zeroes will not be added.</param>
        /// <param name="currentItem">The item containing the index.</param>
        /// <param name="logbook">Optional logging service.</param>
        /// <returns></returns>
        public static string ReplaceNumberPlaceholderWithIndex<T>(
            this string template, 
            T currentItem,
            IEnumerable<T>? items = null,
            ILogbook? logbook = null)
            where T : IIndexed
        {
            logbook?.Write("Replacing number placeholder with index.", LogLevel.Debug);

            string countString = currentItem.Index.ToString();

            if (items != null)
            {
                int numberCount;

                try
                {
                    numberCount = items.Last().Index.ToString().Length;
                }
                catch (Exception ex) when (
                    ex is ArgumentNullException
                    || ex is InvalidOperationException)
                {
                    logbook?.Write($"Items could not be iterated. Returning original string.", LogLevel.Error, ex);

                    return template;
                }

                int zeroCount = numberCount - countString.Length;

                if (zeroCount > 0)
                {
                    countString =
                        string.Concat(Enumerable.Repeat("0", zeroCount))
                        + countString;
                }
            }

            return template.ReplacePlaceholder(Placeholders.Number, countString, logbook);
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
