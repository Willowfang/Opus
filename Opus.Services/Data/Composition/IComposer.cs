using CX.PdfLib.Services;
using Opus.Services.Data.Composition;
using Opus.Services.Input;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data
{
    public interface IComposer
    {
        public Task Compose(string directory, ICompositionProfile compositionProfile,
            bool deleteConverted, bool searchSubDirectories);
    }
}
