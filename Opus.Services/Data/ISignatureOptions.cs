using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data
{
    public interface ISignatureOptions
    {
        /// <summary>
        /// The localized suffix for unsigned files
        /// </summary>
        public string Suffix { get; set; }
    }
}
