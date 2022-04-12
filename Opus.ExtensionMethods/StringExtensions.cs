using Opus.Values;

namespace Opus.ExtensionMethods
{
    public enum Placeholders
    {
        Bookmark,
        File,
        Number
    }

    public static class StringExtensions
    {
        public static string ReplacePlaceholder(this string template, Placeholders placeholder, string replacement)
        {
            string placeholderName = placeholder.ToString();

            foreach (string code in SupportedTypes.CULTURES)
            {
                string? placeholderTemplate = Resources.Placeholders.FileNames.ResourceManager.GetString(placeholderName,
                    new System.Globalization.CultureInfo(code));

                if (placeholderTemplate != null)
                    template = template.Replace(placeholderTemplate, replacement);
            }

            return template;
        }

        public static string ReplaceIllegal(this string original)
        {
            string processed = original.Replace(":", "");
            processed = processed.Replace("/", "-");
            return string.Join("", processed.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
