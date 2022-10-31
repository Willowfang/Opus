using Opus.Services.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Opus.Services.Implementation.Input
{
    /// <summary>
    /// User path selection based on winforms dialogs.
    /// </summary>
    public class PathSelectionWin : IPathSelection
    {
        /// <summary>
        /// Open a single file.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <returns>Selected path or null, if no path selected.</returns>
        public string? OpenFile(string description)
        {
            return OpenDialogFile(description, null);
        }

        /// <summary>
        /// Open a single file.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <param name="fileType">Types of the files to display.</param>
        /// <returns>Selected path or null, if no path selected.</returns>
        public string? OpenFile(string description, FileType fileType)
        {
            return OpenDialogFile(description, GetFilter(fileType));
        }

        /// <summary>
        /// Open a single file.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <param name="fileTypes">Types of the files to display.</param>
        /// <returns>Selected path or null, if no path selected.</returns>
        public string? OpenFile(string description, IEnumerable<FileType> fileTypes)
        {
            return OpenDialogFile(description, GetFilters(fileTypes));
        }

        /// <summary>
        /// Open multiple files.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <returns>Selected paths.</returns>
        public string[] OpenFiles(string description)
        {
            return OpenDialogFile(description, true, null);
        }

        /// <summary>
        /// Open multiple files.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <param name="fileType">Types of the files to display.</param>
        /// <returns>Selected paths.</returns>
        public string[] OpenFiles(string description, FileType fileType)
        {
            return OpenDialogFile(description, true, GetFilter(fileType));
        }

        /// <summary>
        /// Open multiple files.
        /// </summary>
        /// <param name="description">Description to show on the dialog.</param>
        /// <param name="fileTypes">Types of the files to display.</param>
        /// <returns>Selected paths.</returns>
        public string[] OpenFiles(string description, IEnumerable<FileType> fileTypes)
        {
            return OpenDialogFile(description, true, GetFilters(fileTypes));
        }

        /// <summary>
        /// Open a directory.
        /// </summary>
        /// <param name="description">Description to show on the dialog.</param>
        /// <returns>Selected folder path.</returns>
        public string? OpenDirectory(string description)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = description;
            dialog.UseDescriptionForTitle = true;
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.Cancel)
                return null;

            return dialog.SelectedPath;
        }

        /// <summary>
        /// Select a save path.
        /// </summary>
        /// <param name="description">Description to show on the dialog.</param>
        /// <returns>Selected path or null, if not selected.</returns>
        public string? SaveFile(string description)
        {
            return SaveDialogFile(description, null, null, null);
        }

        /// <summary>
        /// Select a save path.
        /// </summary>
        /// <param name="description">Description to show on the dialog.</param>
        /// <param name="initialDirectory">Directory to start the dialog from.</param>
        /// <returns>Selected file path, if path was selected.</returns>
        public string? SaveFile(string description, DirectoryInfo initialDirectory)
        {
            return SaveDialogFile(description, null, initialDirectory, null);
        }

        /// <summary>
        /// Select a save path.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <param name="fileType">Allowed save file types.</param>
        /// <returns>Selected path if any.</returns>
        public string? SaveFile(string description, FileType fileType)
        {
            return SaveDialogFile(description, GetFilter(fileType), null, null);
        }

        /// <summary>
        /// Select a save path.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <param name="fileType">Allowed save file types.</param>
        /// <param name="initialDirectory">Directory to start the search from.</param>
        /// <returns>Selected path, if any.</returns>
        public string? SaveFile(
            string description,
            FileType fileType,
            DirectoryInfo initialDirectory
        )
        {
            return SaveDialogFile(description, GetFilter(fileType), initialDirectory, null);
        }

        /// <summary>
        /// Select save file path.
        /// </summary>
        /// <param name="description">Description to show on the dialog.</param>
        /// <param name="fileType">Allowed save file type.</param>
        /// <param name="initialFile">Initial file suggestion.</param>
        /// <returns>Selected path, if any.</returns>
        public string? SaveFile(string description, FileType fileType, string initialFile)
        {
            return SaveDialogFile(description, GetFilter(fileType), null, initialFile);
        }

        /// <summary>
        /// Select open file path.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <param name="filter">Filter shown filepaths.</param>
        /// <returns>Selected path, if any.</returns>
        private string? OpenDialogFile(string description, string? filter)
        {
            string[] files = OpenDialogFile(description, false, filter);
            return files == null || files.Length == 0 ? null : files[0];
        }

        /// <summary>
        /// Select open file paths.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <param name="multipleSelection">If true, multiple selection is allowed.</param>
        /// <param name="filter">Filter shown files.</param>
        /// <returns>Selected paths, if any.</returns>
        private string[] OpenDialogFile(string description, bool multipleSelection, string? filter)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = description;
            dialog.Multiselect = multipleSelection;
            if (filter != null)
                dialog.Filter = filter;

            dialog.ShowDialog();

            return dialog.FileNames;
        }

        /// <summary>
        /// Select save path.
        /// </summary>
        /// <param name="description">Description shown on the dialog.</param>
        /// <param name="filter">Filter shown files.</param>
        /// <param name="initialDirectory">Directory to start the search from.</param>
        /// <param name="initialFile">Suggested file path.</param>
        /// <returns>Selected path, if any.</returns>
        private string? SaveDialogFile(
            string description,
            string? filter,
            DirectoryInfo? initialDirectory,
            string? initialFile
        )
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = description;
            dialog.Filter = filter;

            if (initialDirectory != null)
                dialog.InitialDirectory = initialDirectory.FullName;
            if (initialFile != null)
            {
                dialog.FileName = initialFile;
            }

            if (dialog.ShowDialog() == false)
                return null;

            return dialog.FileName;
        }

        /// <summary>
        /// Get filters in string form.
        /// </summary>
        /// <param name="fileType">Type of the file to filter with.</param>
        /// <returns>Filters in string form.</returns>
        private string GetFilter(FileType fileType)
        {
            return GetFilters(new List<FileType>() { fileType });
        }

        /// <summary>
        /// Get filters in string form.
        /// </summary>
        /// <param name="fileTypes">Types of the files to filter with.</param>
        /// <returns>Filters in string form.</returns>
        private string GetFilters(IEnumerable<FileType> fileTypes)
        {
            List<string> filters = new List<string>();
            foreach (FileType fileType in fileTypes)
            {
                if (fileType == FileType.Word)
                {
                    filters.Add(
                        FilterString(
                            Resources.Files.FileTypeNames.Doc,
                            Resources.Files.FileExtensions.Doc
                        )
                    );
                    filters.Add(
                        FilterString(
                            Resources.Files.FileTypeNames.Docx,
                            Resources.Files.FileExtensions.Docx
                        )
                    );
                }
                else if (fileType == FileType.PDF)
                {
                    filters.Add(
                        FilterString(
                            Resources.Files.FileTypeNames.Pdf,
                            Resources.Files.FileExtensions.Pdf
                        )
                    );
                }
                else if (fileType == FileType.Profile)
                {
                    filters.Add(
                        FilterString(
                            Resources.Files.FileTypeNames.Profile,
                            Resources.Files.FileExtensions.Profile
                        )
                    );
                }
                else if (fileType == FileType.Zip)
                {
                    filters.Add(
                        FilterString(
                            Resources.Files.FileTypeNames.Zip,
                            Resources.Files.FileExtensions.Zip
                        )
                    );
                }
            }
            return string.Join("|", filters);
        }

        /// <summary>
        /// Get fully formed filter string.
        /// </summary>
        /// <param name="fileType">Type of the file to filter with.</param>
        /// <param name="extension">Extension to filter for.</param>
        /// <returns></returns>
        private string FilterString(string fileType, string extension)
        {
            return String.Join(" ", fileType, "|", "*" + extension);
        }
    }
}
