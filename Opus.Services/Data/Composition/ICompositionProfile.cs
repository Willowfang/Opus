using Opus.Services.Helpers;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data.Composition
{
    /// <summary>
    /// A profile for compositing a pdf-document from
    /// given files
    /// </summary>
    [JsonInterfaceConverter(typeof(InterfaceConverter<ICompositionProfile>))]
    public interface ICompositionProfile : IDataObject
    {
        /// <summary>
        /// Name of the profile
        /// </summary>
        public string ProfileName { get; set; }
        /// <summary>
        /// Segments in the current profile in the order they
        /// are to be included
        /// </summary>
        public ReorderCollection<ICompositionSegment> Segments { get; }
        /// <summary>
        /// If true, page numbers will be added to the new document
        /// </summary>
        public bool AddPageNumbers { get; set; }
        /// <summary>
        /// If true, the user can edit this profile
        /// </summary>
        public bool IsEditable { get; set; }
    }
}
