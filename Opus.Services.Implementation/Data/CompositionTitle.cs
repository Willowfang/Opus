using Opus.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data
{
    public class CompositionTitle : CompositionSegment, ICompositionTitle
    {
        public override string? DisplayName
        {
            get => segmentName;
        }
        private string? segmentName;
        public override string? SegmentName
        {
            get => segmentName;
            set
            {
                SetProperty(ref segmentName, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }
    }
}
