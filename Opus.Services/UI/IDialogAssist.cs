using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.UI
{
    /// <summary>
    /// Provides functionality for associating schemes and
    /// a Prism region with a MaterialDesign in Xaml dialog
    /// </summary>
    public interface IDialogAssist
    {
        /// <summary>
        /// Name of the dialog region
        /// </summary>
        public string DialogRegionName { get; set; }
        /// <summary>
        /// Associate a scheme and a view with the dialog.
        /// </summary>
        /// <typeparam name="T">Type of the view</typeparam>
        /// <param name="schemeName">Scheme to link with</param>
        public void Add<T>(string schemeName);
    }
}
