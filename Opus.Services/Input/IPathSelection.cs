using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Input
{
    public enum FileType
    {
        PDF,
        Word,
        Profile,
        Zip
    }

    /// <summary>
    /// Get user input for file and directory paths
    /// </summary>
    public interface IPathSelection
    {
        /// <summary>
        /// Ask the user for a file path of any file
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <returns>Selected file path. Null, if canceled.</returns>
        public string OpenFile(string description);
        /// <summary>
        /// Ask the user for a file path of a particular file type
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <param name="fileType">Type of the file</param>
        /// <returns>Selected file path. Null, if canceled.</returns>
        public string OpenFile(string description, FileType fileType);
        /// <summary>
        /// Ask the user for a file path of a file of any of the given types
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <param name="fileTypes">Accepted types</param>
        /// <returns>Selected file path. Null, if canceled.</returns>
        public string OpenFile(string description, IEnumerable<FileType> fileTypes);
        /// <summary>
        /// Ask the user for one or more file paths
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <returns>Selected file paths. Empty, if canceled.</returns>
        public string[] OpenFiles(string description);
        /// <summary>
        /// Ask the user for one or more file paths of a given type
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <param name="fileType">Type of accepted files</param>
        /// <returns>Selected file paths. Empty, if canceled.</returns>
        public string[] OpenFiles(string description, FileType fileType);
        /// <summary>
        /// Ask the user for one or more file paths of any of the given types
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <param name="fileTypes">Types of accepted files</param>
        /// <returns>Selected file paths. Empty, if canceled.</returns>
        public string[] OpenFiles(string description, IEnumerable<FileType> fileTypes);
        /// <summary>
        /// Ask the user for a directory path
        /// </summary>
        /// <param name="description">Description for directory selection</param>
        /// <returns>Selected directory path. Null, if canceled.</returns>
        public string OpenDirectory(string description);
        /// <summary>
        /// Ask the user for a file path for saving a file of any type
        /// </summary>
        /// <param name="description">Description of file selection</param>
        /// <returns>Selected file path. Null, if canceled.</returns>
        public string SaveFile(string description);
        /// <summary>
        /// Ask the user for a file path for saving a file of any type
        /// </summary>
        /// <param name="description">Description of file selection</param>
        /// <param name="initialDirectory">Directory to start the selection from</param>
        /// <returns>Selected file path. Null, if canceled.</returns>
        public string SaveFile(string description, DirectoryInfo initialDirectory);
        /// <summary>
        /// Ask the user for a file path for saving a file of a particular type
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <param name="fileType">Type of accepted file</param>
        /// <returns>Selected file path. Null, if canceled.</returns>
        public string SaveFile(string description, FileType fileType);
        /// <summary>
        /// Ask the user for a file path for saving a file of a particular type
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <param name="fileType">Type of accepted file</param>
        /// <param name="initialDirectory">Directory to start the selection from</param>
        /// <returns>Selected file path. Null, if canceled.</returns>
        public string SaveFile(string description, FileType fileType, DirectoryInfo initialDirectory);
        /// <summary>
        /// Ask the user for a file path for saving a file of a particular type
        /// </summary>
        /// <param name="description">Description for file selection</param>
        /// <param name="fileType">Type of accepted file</param>
        /// <param name="initialFile">Suggestion file location and name</param>
        /// <returns>Selected file path. Null, if canceled.</returns>
        public string SaveFile(string description, FileType fileType, string initialFile);
    }
}
