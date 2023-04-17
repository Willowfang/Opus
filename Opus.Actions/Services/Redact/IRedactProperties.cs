using Opus.Actions.Services.Base;
using Opus.Common.Wrappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.PdfLib.Common.Redaction;

namespace Opus.Actions.Services.Redact
{
    /// <summary>
    /// Properties for redaction action.
    /// </summary>
    public interface IRedactProperties : IActionProperties
    {
        /// <summary>
        /// Files to apply redactions to.
        /// </summary>
        public ObservableCollection<FileStorage> Files { get; }

        /// <summary>
        /// Currently selected file.
        /// </summary>
        public FileStorage? SelectedFile { get; set; }

        /// <summary>
        /// Words to redact from the documents (may use wildcards * and ?) separated by comma.
        /// </summary>
        public string? WordsToRedact { get; set; }
    }
}
