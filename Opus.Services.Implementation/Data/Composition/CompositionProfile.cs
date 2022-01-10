using Opus.Services.Data;
using Opus.Services.Data.Composition;
using Opus.Services.UI;
using Prism.Mvvm;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// Concrete implementation of <see cref="ICompositionProfile"/>.
    /// </summary>
    internal class CompositionProfile : DataObject<CompositionProfile>, IDataObject, ICompositionProfile
    {
        private string? profileName;
        public string? ProfileName
        {
            get => profileName;
            set
            {
                SetProperty(ref profileName, value);
            }
        }

        private ReorderCollection<ICompositionSegment>? segments;
        public ReorderCollection<ICompositionSegment>? Segments
        {
            get => segments;
            set => SetProperty(ref segments, value);
        }

        private bool addPageNumbers;
        public bool AddPageNumbers
        {
            get => addPageNumbers;
            set
            {
                SetProperty(ref addPageNumbers, value);
            }
        }

        private bool isEditable;
        public bool IsEditable
        {
            get => isEditable;
            set
            {
                SetProperty(ref isEditable, value);
                if (Segments is not null) Segments.CanReorder = value;
            }
        }

        public override int GetHashCode()
        {
            return ProfileName != null ? ProfileName.GetHashCode() : Id.GetHashCode();
        }

        protected override bool CheckEquality(CompositionProfile current, CompositionProfile other)
        {
            return current.Id == other.Id || current.ProfileName == other.ProfileName;
        }
    }
}
