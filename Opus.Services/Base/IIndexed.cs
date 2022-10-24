using CX.PdfLib.Services.Data;

namespace Opus.Services.Base
{
    /// <summary>
    /// An item that has an index (used in lists).
    /// <para>
    /// Inherits ILeveledItem, so needs to also have
    /// a level property.
    /// </para>
    /// </summary>
    public interface IIndexed : ILeveledItem
    {
        /// <summary>
        /// Index of the item in an ordered list.
        /// </summary>
        public int Index { get; set; }
    }
}
