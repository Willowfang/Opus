using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.Wrappers
{
    public class LanguageOption
    {
        public string Code { get; }
        public string Name { get; }

        public LanguageOption(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }
}
