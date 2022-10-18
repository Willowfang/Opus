using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Base
{
    public interface IIndexed : ILeveledItem
    {
        public int Index { get; set; }
    }
}
