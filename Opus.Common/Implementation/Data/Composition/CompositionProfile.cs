using Opus.Common.Services.Data;
using Opus.Common.Services.Data.Composition;
using Opus.Common.Collections;

namespace Opus.Common.Implementation.Data.Composition
{
    /// <summary>
    /// Default implementation of <see cref="ICompositionProfile"/>.
    /// </summary>
    internal class CompositionProfile
        : DataObject<CompositionProfile>,
            IDataObject,
            ICompositionProfile
    {
        #region Fields and properties
        private string? profileName;

        /// <summary>
        /// Name of this profile.
        /// </summary>
        public string? ProfileName
        {
            get => profileName;
            set { SetProperty(ref profileName, value); }
        }

        private ReorderCollection<ICompositionSegment>? segments;

        /// <summary>
        /// Segments contained in this profile.
        /// </summary>
        public ReorderCollection<ICompositionSegment>? Segments
        {
            get => segments;
            set => SetProperty(ref segments, value);
        }

        private bool addPageNumbers;

        /// <summary>
        /// If true, page numbers will be added after composition.
        /// </summary>
        public bool AddPageNumbers
        {
            get => addPageNumbers;
            set { SetProperty(ref addPageNumbers, value); }
        }

        private bool isEditable;

        /// <summary>
        /// If true, user may edit this profile.
        /// </summary>
        public bool IsEditable
        {
            get => isEditable;
            set
            {
                SetProperty(ref isEditable, value);
                if (Segments is not null)
                    Segments.CanReorder = value;
            }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ProfileName != null ? ProfileName.GetHashCode() : Id.GetHashCode();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        protected override bool CheckEquality(CompositionProfile current, CompositionProfile other)
        {
            return current.Id == other.Id || current.ProfileName == other.ProfileName;
        }
        #endregion
    }
}
