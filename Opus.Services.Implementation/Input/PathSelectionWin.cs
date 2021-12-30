using Opus.Services.Input;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Opus.Services.Implementation.Input
{
    /// <summary>
    /// User path selection based on winforms dialogs
    /// </summary>
    public class PathSelectionWin : IPathSelection
    {
        public string? OpenFile(string description)
        {
            return OpenDialogFile(description, null);
        }
        public string? OpenFile(string description, FileType fileType)
        {
            return OpenDialogFile(description, GetFilter(fileType));
        }
        public string? OpenFile(string description, IEnumerable<FileType> fileTypes)
        {
            return OpenDialogFile(description, GetFilters(fileTypes));
        }

        public string[] OpenFiles(string description)
        {
            return OpenDialogFile(description, true, null);
        }
        public string[] OpenFiles(string description, FileType fileType)
        {
            return OpenDialogFile(description, true, GetFilter(fileType));
        }
        public string[] OpenFiles(string description, IEnumerable<FileType> fileTypes)
        {
            return OpenDialogFile(description, true, GetFilters(fileTypes));
        }

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

        public string? SaveFile(string description)
        {
            return SaveDialogFile(description, null, null, null);
        }
        public string? SaveFile(string description, DirectoryInfo initialDirectory)
        {
            return SaveDialogFile(description, null, initialDirectory, null);
        }
        public string? SaveFile(string description, FileType fileType)
        {
            return SaveDialogFile(description, GetFilter(fileType), null, null);
        }
        public string? SaveFile(string description, FileType fileType, DirectoryInfo initialDirectory)
        {
            return SaveDialogFile(description, GetFilter(fileType), initialDirectory, null);
        }
        public string? SaveFile(string description, FileType fileType, string initialFile)
        {
            return SaveDialogFile(description, GetFilter(fileType), null, initialFile);
        }

        private string? OpenDialogFile(string description, string? filter)
        {
            string[] files = OpenDialogFile(description, false, filter);
            return files == null || files.Length == 0 ? null : files[0];
        }
        private string[] OpenDialogFile(string description, bool multipleSelection, string? filter)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = description;
            dialog.Multiselect = multipleSelection;
            if (filter != null) dialog.Filter = filter;

            dialog.ShowDialog();

            return dialog.FileNames;
        }
        
        private string? SaveDialogFile(string description, string? filter, DirectoryInfo? initialDirectory,
            string? initialFile)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = description;
            dialog.Filter = filter;
            if (initialDirectory != null) dialog.InitialDirectory = initialDirectory.FullName;
            if (initialFile != null)
            {
                dialog.FileName = initialFile;
            }

            if (dialog.ShowDialog() == false)
                return null;

            return dialog.FileName;
        }

        private string GetFilter(FileType fileType)
        {
            return GetFilters(new List<FileType>() { fileType });
        }
        private string GetFilters(IEnumerable<FileType> fileTypes)
        {
            List<string> filters = new List<string>();
            foreach (FileType fileType in fileTypes)
            {
                if (fileType == FileType.PDF)
                    filters.Add("PDF | *.pdf");
                else if (fileType == FileType.Word)
                {
                    filters.Add("Word | *.doc");
                    filters.Add("Word | *.docx");
                }
            }
            return string.Join("|", filters);
        }
    }
}
