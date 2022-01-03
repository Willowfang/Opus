using Opus.Services.Data;
using Opus.Services.Data.Composition;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Opus.Services.Implementation.Data.Composition
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

        public bool ExportProfile(ICompositionProfile profile, string filePath)
        {
            if (Path.GetExtension(filePath) != Resources.Files.FileExtensions.Profile)
                throw new ArgumentException(nameof(filePath));

            try
            {
                string json = JsonSerializer.Serialize(profile);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ICompositionProfile? ImportProfile(string filePath)
        {
            if (Path.GetExtension(filePath) != Resources.Files.FileExtensions.Profile)
                throw new ArgumentException(nameof(filePath));

            try
            {
                string json = File.ReadAllText(filePath);
                ICompositionProfile? profile = JsonSerializer.Deserialize<ICompositionProfile>(json);
                return profile;
            }
            catch
            {
                throw new IOException(nameof(ICompositionProfile));
            }
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
