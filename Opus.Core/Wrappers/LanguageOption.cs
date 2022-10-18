using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.Wrappers
{
    /// <summary>
    /// A storage class for a language. These options are displayed in the language menu
    /// and can be chosen by the user.
    /// </summary>
    public class LanguageOption
    {
        /// <summary>
        /// Two-letter language code (e.g. "en").
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Name of the language to display (e.g. "English").
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Create a new language option.
        /// </summary>
        /// <param name="code">Two-letter language code.</param>
        /// <param name="name">Name of the language to display.</param>
        public LanguageOption(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }
}
