using CX.LoggingLib;
using Opus.Services.Data;
using Opus.Services.Data.Composition;
using Opus.Services.Implementation.Logging;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// Composition options default implementation.
    /// </summary>
    public class CompositionOptions : LoggingCapable<CompositionOptions>, ICompositionOptions
    {
        #region DI services
        private IDataProvider dataProvider;
        #endregion

        #region Constructor
        /// <summary>
        /// Create new composition options.
        /// </summary>
        /// <param name="dataProvider">Service providing access to local database.</param>
        /// <param name="logbook">Logging service.</param>
        public CompositionOptions(IDataProvider dataProvider, ILogbook logbook) : base(logbook)
        {
            this.dataProvider = dataProvider;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create a new <see cref="ICompositionProfile"/>
        /// with default settings.
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
        public ICompositionProfile CreateProfile(string name, bool addPageNumbers, bool isEditable)
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
        public ICompositionProfile CreateProfile(
            string name,
            bool addPageNumbers,
            bool isEditable,
            List<ICompositionSegment> segments
        )
        {
            return new CompositionProfile()
            {
                ProfileName = name,
                AddPageNumbers = addPageNumbers,
                IsEditable = isEditable,
                Segments = new ReorderCollection<ICompositionSegment>(segments)
            };
        }

        /// <summary>
        /// Return all profiles stored in the database.
        /// </summary>
        /// <returns>All profiles found.</returns>
        public IList<ICompositionProfile> GetProfiles()
        {
            return dataProvider.GetAll<ICompositionProfile>();
        }

        /// <summary>
        /// Save a profile into the database.
        /// </summary>
        /// <param name="profile">Profile to save.</param>
        /// <returns>Profile that was saved.</returns>
        public ICompositionProfile SaveProfile(ICompositionProfile profile)
        {
            return dataProvider.Save(profile);
        }

        /// <summary>
        /// Remove a profile from the database.
        /// </summary>
        /// <param name="profile">Profile to remove.</param>
        public void DeleteProfile(ICompositionProfile profile)
        {
            dataProvider.Delete(profile);
        }

        /// <summary>
        /// Export a given profile as a JSON file.
        /// </summary>
        /// <param name="profile">Profile to export.</param>
        /// <param name="filePath">Filepath to export the profile to.</param>
        /// <returns>Info on success.</returns>
        /// <exception cref="ArgumentException">Thrown, if given filepath's extension is wrong.</exception>
        public bool ExportProfile(ICompositionProfile profile, string filePath)
        {
            if (Path.GetExtension(filePath) != Resources.Files.FileExtensions.Profile)
                throw new ArgumentException(filePath);

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(profile, options);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (ArgumentException e)
            {
                logbook.Write(
                    $"Saving of {nameof(ICompositionProfile)} '{profile.ProfileName}' to {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                return false;
            }
            catch (NotSupportedException e)
            {
                logbook.Write(
                    $"JSON conversion of {nameof(ICompositionProfile)} '{profile.ProfileName}' failed.",
                    LogLevel.Error,
                    e
                );
                return false;
            }
            catch (PathTooLongException e)
            {
                logbook.Write(
                    $"Saving of {nameof(ICompositionProfile)} '{profile.ProfileName}' to {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                return false;
            }
            catch (IOException e)
            {
                logbook.Write(
                    $"Saving of {nameof(ICompositionProfile)} '{profile.ProfileName}' to {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                return false;
            }
            catch (UnauthorizedAccessException e)
            {
                logbook.Write(
                    $"Saving of {nameof(ICompositionProfile)} '{profile.ProfileName}' to {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                return false;
            }
        }

        /// <summary>
        /// Import a profile from a file.
        /// </summary>
        /// <param name="filePath">Path of the file to import from.</param>
        /// <returns>Imported profile (if successful).</returns>
        /// <exception cref="ArgumentException">Thrown, if file extension of given file is wrong.</exception>
        public ICompositionProfile? ImportProfile(string filePath)
        {
            if (Path.GetExtension(filePath) != Resources.Files.FileExtensions.Profile)
                throw new ArgumentException(filePath);

            ICompositionProfile? profile = null;
            try
            {
                string json = File.ReadAllText(filePath);
                profile = JsonSerializer.Deserialize<ICompositionProfile>(json);
                return profile;
            }
            catch (ArgumentException e)
            {
                logbook.Write(
                    $"Importing {nameof(ICompositionProfile)} from {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                throw;
            }
            catch (NotSupportedException e)
            {
                logbook.Write(
                    $"JSON deserialization of {nameof(ICompositionProfile)} {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                throw;
            }
            catch (JsonException e)
            {
                logbook.Write(
                    $"JSON deserialization of {nameof(ICompositionProfile)} {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                throw;
            }
            catch (PathTooLongException e)
            {
                logbook.Write(
                    $"Importing {nameof(ICompositionProfile)} from {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                throw;
            }
            catch (IOException e)
            {
                logbook.Write(
                    $"Importing {nameof(ICompositionProfile)} from {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                throw;
            }
            catch (UnauthorizedAccessException e)
            {
                logbook.Write(
                    $"Importing {nameof(ICompositionProfile)} from {filePath} failed.",
                    LogLevel.Error,
                    e
                );
                throw;
            }
        }

        /// <summary>
        /// Create a new file segment.
        /// </summary>
        /// <param name="segmentName">Name of the new segment.</param>
        /// <returns>Created segment.</returns>
        public ICompositionFile CreateFileSegment(string segmentName)
        {
            return new CompositionFile(segmentName);
        }

        /// <summary>
        /// Create a new title segment.
        /// </summary>
        /// <param name="segmentName">Name of the new segment.</param>
        /// <returns>The created segment.</returns>
        public ICompositionTitle CreateTitleSegment(string segmentName)
        {
            return new CompositionTitle(segmentName);
        }
        #endregion
    }
}
