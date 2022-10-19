using Opus.Services.Data.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// Default implementation for a title segment.
    /// </summary>
    public class CompositionTitle : CompositionSegment, ICompositionTitle
    {
        /// <summary>
        /// Name of this structure (just use displayName).
        /// </summary>
        public override string? StructureName => DisplayName;

        /// <summary>
        /// Name to show to the user.
        /// </summary>
        public override string? DisplayName
        {
            get => segmentName;
        }
        private string? segmentName;

        /// <summary>
        /// Name for internal purposes.
        /// </summary>
        public override string? SegmentName
        {
            get => segmentName;
            set
            {
                SetProperty(ref segmentName, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// Create a new title segment.
        /// </summary>
        public CompositionTitle() { }

        /// <summary>
        /// Create a new title segment with a given name.
        /// </summary>
        /// <param name="segmentName"></param>
        public CompositionTitle(string segmentName)
        {
            SegmentName = segmentName;
            Level = 1;
        }
    }
}
