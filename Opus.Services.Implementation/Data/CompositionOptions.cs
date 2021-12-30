using Opus.Services.Configuration;
using Opus.Services.Data;
using Opus.Services.Implementation.UI;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data
{
    public class CompositionOptions : ICompositionOptions
    {
        private IDataProvider dataProvider;

        public CompositionOptions(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        #region PROFILES

        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// with default settings
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <returns></returns>
        public ICompositionProfile CreateProfile(string name)
        {
            return CreateProfile(name, true, true);
        }

        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// with an empty list of segments
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <param name="addPageNumbers">Add page numbers to final document</param>
        /// <param name="isEditable">The user can edit the profile</param>
        /// <returns></returns>
        public ICompositionProfile CreateProfile(string name, bool addPageNumbers,
            bool isEditable)
        {
            return CreateProfile(name, addPageNumbers, isEditable, new List<ICompositionSegment>());
        }

        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <param name="addPageNumbers">Add page numbers to final document</param>
        /// <param name="isEditable">The user can edit the profile</param>
        /// <param name="segments">Segments in the profile</param>
        /// <returns></returns>
        public ICompositionProfile CreateProfile(string name, bool addPageNumbers,
            bool isEditable, List<ICompositionSegment> segments)
        {
            return new CompositionProfile()
            {
                ProfileName = name,
                AddPageNumbers = addPageNumbers,
                IsEditable = isEditable,
                Segments = new ReorderCollection<ICompositionSegment>(segments)
            };
        }

        public IList<ICompositionProfile> GetProfiles()
        {
            return dataProvider.GetAll<ICompositionProfile>();
        }

        public ICompositionProfile SaveProfile(ICompositionProfile profile)
        {
            return dataProvider.Save(profile);
        }

        public void DeleteProfile(ICompositionProfile profile)
        {
            dataProvider.Delete(profile);
        }

        #endregion

        #region SEGMENTS

        public ICompositionFile CreateFileSegment(string segmentName)
        {
            return new CompositionFile(segmentName);
        }

        public ICompositionTitle CreateTitleSegment(string segmentName)
        {
            return new CompositionTitle(segmentName);
        }

        #endregion
    }
}
