using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.UI
{
    public interface IDialogAssist
    {
        /// <summary>
        /// Indicates whether the dialog is visible
        /// </summary>
        public bool IsShowing { get; }
        /// <summary>
        /// Currently visible dialog
        /// </summary>
        public IDialog Active { get; }
        /// <summary>
        /// Show a dialog asynchronously and return it when ready
        /// </summary>
        /// <param name="dialog">Dialog to show</param>
        public Task<IDialog> Show(IDialog dialog);
    }
}
