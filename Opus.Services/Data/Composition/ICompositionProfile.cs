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
    /// A profile for composing a pdf-document from various files and titles.
    /// Provides instructions on how to compose the final file. May be exported
    /// to a JSON-based file and imported from said file type.
    /// </summary>
    [JsonInterfaceConverter(typeof(InterfaceConverter<ICompositionProfile>))]
    public interface ICompositionProfile : IDataObject
    {
        /// <summary>
        /// Name of the profile.
        /// </summary>
        public string ProfileName { get; set; }
        /// <summary>
        /// Segments in the current profile in the order they
        /// are to be included. Segments include information on the title,
        /// search criteria and number of files to include, among other things.
        /// </summary>
        public ReorderCollection<ICompositionSegment> Segments { get; }
        /// <summary>
        /// If true, page numbers will be added to the final product.
        /// </summary>
        public bool AddPageNumbers { get; set; }
        /// <summary>
        /// If true, the user can edit this profile. Uneditable profiles can be created by the
        /// administrator to prevent users from changing their content. This property does not
        /// prevent tampering with the profile directly in the database.
        /// </summary>
        public bool IsEditable { get; set; }
    }
}
