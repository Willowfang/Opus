using Opus.Common.Logging;
using Opus.Common.Services.Base;
using Prism.Mvvm;

namespace Opus.Common.Implementation.Base
{
    /// <summary>
    /// A base class for selectable items. Provides notifications.
    /// </summary>
    public abstract class SelectableBase : BindableBase, ISelectable
    {
        private bool isSelected;

        /// <summary>
        /// If true, the item is selected.
        /// <para>
        /// Provides notifications.
        /// </para>
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }
    }
}
