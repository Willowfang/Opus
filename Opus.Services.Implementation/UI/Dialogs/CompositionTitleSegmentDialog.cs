using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class CompositionTitleSegmentDialog : DialogBase, IDataErrorInfo
    {
        private string? segmentName;
        public string? SegmentName
        {
            get => segmentName;
            set => SetProperty(ref segmentName, value);
        }

        public CompositionTitleSegmentDialog(string dialogTitle)
            : base(dialogTitle) { }

        public string? Error
        {
            get => null;
        }

        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(SegmentName))
                {
                    if (string.IsNullOrEmpty(SegmentName))
                        return Resources.Validation.General.NameEmpty;
                }

                return string.Empty;
            }
        }
    }
}
