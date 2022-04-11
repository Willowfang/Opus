using CX.PdfLib.Common;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.UI
{
    public class ProgressContainer
    {
        public Task Show { get; }
        public IDialog ProgressDialog { get; }
        public IProgress<ProgressReport> Reporting { get; }

        public ProgressContainer(Task show, IDialog progressDialog,
            IProgress<ProgressReport> reporting)
        {
            Show = show;
            ProgressDialog = progressDialog;
            Reporting = reporting;
        }
    }
}
