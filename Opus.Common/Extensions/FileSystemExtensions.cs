using System.IO;

namespace Opus.Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="FileSystemInfo"/>.
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Open a pdf file or, if the parameter is of another file type or a folder,
        /// the folder for viewing.
        /// </summary>
        /// <param name="info">Info to open.</param>
        /// <returns>True, if the file or folder could be opened, otherwise false.</returns>
        public static bool OpenPdfOrDirectory(this FileSystemInfo info)
        {
            string? destination = null;

            if (info is FileInfo file)
            {
                if (file.Extension.ToLower() == Resources.Files.FileExtensions.Pdf)
                {
                    destination = file.FullName;
                }
                else if (file.Directory != null)
                {
                    destination = file.Directory.FullName;
                }
            }
            else if (info is DirectoryInfo directory)
            {
                destination = directory.FullName;
            }

            if (destination == null)
            {
                return false; 
            }

            new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo(
                    destination)
                {
                    UseShellExecute = true
                }
            }.Start();

            return true;
        }
    }
}
