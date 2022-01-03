using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.UI
{
    /// <summary>
    /// Service for navigation targets of <see cref="INavigationAssist"/>
    /// </summary>
    public interface INavigationTarget
    {
        /// <summary>
        /// Operations to perform when navigated to by <see cref="INavigationAssist"/>
        /// </summary>
        public void OnArrival();
        /// <summary>
        /// Operations to perform when navigated from by <see cref="INavigationAssist"/>
        /// </summary>
        public void WhenLeaving();
    }
}
