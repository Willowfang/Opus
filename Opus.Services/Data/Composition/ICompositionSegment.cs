using CX.PdfLib.Services.Data;
using Opus.Services.Helpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Opus.Services.Data.Composition
{
    /// <summary>
    /// A segment of a <see cref="ICompositionProfile"/>
    /// </summary>
    [JsonInterfaceConverter(typeof(InterfaceConverter<ICompositionSegment>))]
    public interface ICompositionSegment : ILeveledItem
    {
        /// <summary>
        /// Name to display to the user
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Name of the segment
        /// </summary>
        public string SegmentName { get; set; }
    }
}
