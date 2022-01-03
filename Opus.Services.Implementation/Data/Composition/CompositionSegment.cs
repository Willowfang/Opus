using Opus.Services.Data.Composition;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data.Composition
{
    public abstract class CompositionSegment : BindableBase, ICompositionSegment
    {
        public abstract string? StructureName { get; }
        public abstract string? DisplayName { get; }
        public abstract string? SegmentName { get; set; }

        private int level;
        public int Level
        {
            get => level;
            set => SetProperty(ref level, value);
        }
    }
}
