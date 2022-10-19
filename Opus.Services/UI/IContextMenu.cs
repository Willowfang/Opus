using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.UI
{
    /// <summary>
    /// Context menu service definition.
    /// </summary>
    public interface IContextMenu
    {
        /// <summary>
        /// Run the selected context menu action.
        /// </summary>
        /// <param name="arguments">Arguments to run with.</param>
        /// <returns>An awaitable task.</returns>
        public Task Run(string[] arguments);
    }
}
