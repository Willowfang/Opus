using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionLib
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces illegal filepath characters in a string and returns a new string
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string ReplaceIllegal(this string original)
        {
            string processed = original.Replace(":", "");
            processed = processed.Replace("/", "-");
            return string.Join("", processed.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
