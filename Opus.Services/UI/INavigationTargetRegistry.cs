using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.UI
{
    public interface INavigationTargetRegistry
    {
        public void AddTarget(string schemeName, INavigationTarget target);
    }
}
