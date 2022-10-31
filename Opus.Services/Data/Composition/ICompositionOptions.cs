using System.Collections.Generic;

namespace Opus.Services.Data.Composition
{
    /// <summary>
    /// Options used in compositions.
    /// </summary>
    public interface ICompositionOptions
    {
        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// with default settings
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <returns>The created profile.</returns>
        public ICompositionProfile CreateProfile(string name);

        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// with an empty list of segments
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <param name="addPageNumbers">Add page numbers to final document</param>
        /// <param name="isEditable">The user can edit the profile</param>
        /// <returns>The created profile.</returns>
        public ICompositionProfile CreateProfile(string name, bool addPageNumbers, bool isEditable);

        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <param name="addPageNumbers">Add page numbers to final document</param>
        /// <param name="isEditable">The user can edit the profile</param>
        /// <param name="segments">Segments in the profile</param>
        /// <returns>The created profile.</returns>
        public ICompositionProfile CreateProfile(
            string name,
            bool addPageNumbers,
            bool isEditable,
            List<ICompositionSegment> segments
        );

        /// <summary>
        /// Return all <see cref="ICompositionProfile"/>s from the data provider
        /// </summary>
        /// <returns>All found profiles.</returns>
        public IList<ICompositionProfile> GetProfiles();

        /// <summary>
        /// Save the <see cref="ICompositionProfile"/> with the data provider
        /// </summary>
        /// <param name="profile">Profile to save</param>
        /// <returns>The saved profile.</returns>
        public ICompositionProfile SaveProfile(ICompositionProfile profile);

        /// <summary>
        /// Delete an <see cref="ICompositionProfile"/> via the data provider
        /// </summary>
        /// <param name="profile">Profile to delete</param>
        public void DeleteProfile(ICompositionProfile profile);

        /// <summary>
        /// Import a profile into the program.
        /// </summary>
        /// <param name="filePath">Path to import the profile from.</param>
        /// <returns>The imported profile.</returns>
        public ICompositionProfile ImportProfile(string filePath);
        public bool ExportProfile(ICompositionProfile profile, string filePath);

        /// <summary>
        /// Create a new <see cref="ICompositionSegment"/> for files
        /// </summary>
        /// <param name="segmentName">Name of the segment</param>
        /// <returns>The created segment.</returns>
        public ICompositionFile CreateFileSegment(string segmentName);

        /// <summary>
        /// Create a new <see cref="ICompositionSegment"/> for a title
        /// </summary>
        /// <param name="segmentName">Name of the segment</param>
        /// <returns>The created segment.</returns>
        public ICompositionTitle CreateTitleSegment(string segmentName);
    }
}
