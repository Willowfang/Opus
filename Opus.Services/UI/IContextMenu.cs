using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.UI
{
    public interface IContextMenu
    {
        public Task Run(string[] arguments);
    }
}
