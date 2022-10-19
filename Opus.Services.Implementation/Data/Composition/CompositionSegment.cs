using Opus.Services.Data.Composition;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// Default abstract base class for general composition segment.
    /// </summary>
    public abstract class CompositionSegment : BindableBase, ICompositionSegment
    {
        /// <summary>
        /// Name of this structure.
        /// </summary>
        public abstract string? StructureName { get; }

        /// <summary>
        /// Name for displaying to the user.
        /// </summary>
        public abstract string? DisplayName { get; }

        /// <summary>
        /// Name for internal purposes.
        /// </summary>
        public abstract string? SegmentName { get; set; }

        private int level;

        /// <summary>
        /// Level of this segment in the hierarchy.
        /// </summary>
        public int Level
        {
            get => level;
            set => SetProperty(ref level, value);
        }
    }
}
