using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data.Composition
{
    public interface ICompositionOptions
    {
        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// with default settings
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <returns></returns>
        public ICompositionProfile CreateProfile(string name);
        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// with an empty list of segments
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <param name="addPageNumbers">Add page numbers to final document</param>
        /// <param name="isEditable">The user can edit the profile</param>
        /// <returns></returns>
        public ICompositionProfile CreateProfile(string name, bool addPageNumbers,
            bool isEditable);
        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <param name="addPageNumbers">Add page numbers to final document</param>
        /// <param name="isEditable">The user can edit the profile</param>
        /// <param name="segments">Segments in the profile</param>
        /// <returns></returns>
        public ICompositionProfile CreateProfile(string name, bool addPageNumbers,
            bool isEditable, List<ICompositionSegment> segments);
        /// <summary>
        /// Return all <see cref="ICompositionProfile"/>s from the data provider
        /// </summary>
        /// <returns></returns>
        public IList<ICompositionProfile> GetProfiles();
        /// <summary>
        /// Save the <see cref="ICompositionProfile"/> with the data provider
        /// </summary>
        /// <param name="profile">Profile to save</param>
        /// <returns></returns>
        public ICompositionProfile SaveProfile(ICompositionProfile profile);
        /// <summary>
        /// Delete an <see cref="ICompositionProfile"/> via the data provider
        /// </summary>
        /// <param name="profile">Profile to delete</param>
        public void DeleteProfile(ICompositionProfile profile);

        public ICompositionProfile ImportProfile(string filePath);
        public bool ExportProfile(ICompositionProfile profile, string filePath);

        /// <summary>
        /// Create a new <see cref="ICompositionSegment"/> for files
        /// </summary>
        /// <param name="segmentName">Name of the segment</param>
        /// <returns></returns>
        public ICompositionFile CreateFileSegment(string segmentName);
        /// <summary>
        /// Create a new <see cref="ICompositionSegment"/> for a title
        /// </summary>
        /// <param name="segmentName">Name of the segment</param>
        /// <returns></returns>
        public ICompositionTitle CreateTitleSegment(string segmentName);
    }
}
