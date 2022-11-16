namespace Opus.Common.Services.Base
{
    /// <summary>
    /// An item that is selectable.
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// If true, item is selected.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
