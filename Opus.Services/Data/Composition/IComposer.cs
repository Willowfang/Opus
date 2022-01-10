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
    public interface IComposerFactory
    {
        public IComposer Create();
    }

    public interface IComposer 
    {
        public Task Compose(string directory, ICompositionProfile profile, bool deleteConverted,
            bool searchSubDirectories);
    }
}
