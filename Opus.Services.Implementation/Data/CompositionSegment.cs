using Opus.Services.Data;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data
{
    public abstract class CompositionSegment : BindableBase, ICompositionSegment
    {
        public abstract string? DisplayName { get; }
        public abstract string? SegmentName { get; set; }
        public int Level { get; set; }
    }
}
