using CX.LoggingLib;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// A dialog for choosing work copy settings.
    /// </summary>
    public class WorkCopySettingsDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private string? titleTemplate;

        /// <summary>
        /// Template string for product names.
        /// </summary>
        public string? TitleTemplate
        {
            get => titleTemplate;
            set { SetProperty(ref titleTemplate, value); }
        }

        private bool flattenRedactions;

        /// <summary>
        /// If true, readactions will be turned to red rectangles.
        /// </summary>
        public bool FlattenRedactions
        {
            get => flattenRedactions;
            set => SetProperty(ref flattenRedactions, value);
        }

        /// <summary>
        /// Validatoin error. Always return null.
        /// </summary>
        public string? Error
        {
            get => null;
        }

        /// <summary>
        /// Create a new dialog for choosing work copy settings.
        /// </summary>
        /// <param name="dialogTitle"></param>
        public WorkCopySettingsDialog(string dialogTitle) : base(dialogTitle) { }

        /// <summary>
        /// Validation.
        /// </summary>
        /// <param name="propertyName">Property to validate.</param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(TitleTemplate))
                {
                    if (string.IsNullOrEmpty(titleTemplate))
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                }

                return string.Empty;
            }
        }
    }
}
