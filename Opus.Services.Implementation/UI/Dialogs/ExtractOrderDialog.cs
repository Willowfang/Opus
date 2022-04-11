using CX.LoggingLib;
using CX.PdfLib.Services.Data;
using Opus.Services.Implementation.Data.Extraction;
using Opus.Services.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class ExtractOrderDialog : DialogBase, IDialog
    {
        public ReorderCollection<FileAndBookmarkWrapper> Bookmarks { get; }

        public bool SingleFile { get; }

        private bool groupByFiles;
        public bool GroupByFiles
        {
            get => groupByFiles;
            set => SetProperty(ref groupByFiles, value);
        }

        public ExtractOrderDialog(string title, bool singleFile, bool groupByFiles)
            : base(title)
        {
            Bookmarks = new ReorderCollection<FileAndBookmarkWrapper>();
            Bookmarks.CanReorder = true;
            SingleFile = singleFile;
            GroupByFiles = groupByFiles;
        }
    }
}
