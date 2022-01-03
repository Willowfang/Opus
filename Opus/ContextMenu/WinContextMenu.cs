using Opus.Services.Configuration;
using CX.PdfLib.Services;
using System.IO;
using System.Windows.Forms;
using CX.PdfLib.Services.Data;
using System.Collections.Generic;
using Opus.Services.Data;

namespace Opus.ContextMenu
{
    public interface IContextMenuCommand
    {
        public void RunCommand(string[] parameters);
    }

    internal abstract class MenuCommandBase : IContextMenuCommand
    {
        protected IManipulator Manipulator;

        public abstract void RunCommand(string[] parameters);
    }

    internal abstract class MenuCommandExtractBase : MenuCommandBase
    {
        protected IList<ILeveledBookmark> GetBookmarks(string filePath)
        {
            return Manipulator.FindBookmarks(filePath);
        }

        protected IList<ILeveledBookmark> GetBookmarks(string filePath, string preFix)
        {
            List<ILeveledBookmark> selected = new List<ILeveledBookmark>();
            foreach (ILeveledBookmark bookmark in Manipulator.FindBookmarks(filePath))
            {
                if (bookmark.Title.ToLower().StartsWith(preFix.ToLower()))
                {
                    selected.Add(bookmark);
                }
            }

            return selected;
        }
    }

    internal class RemoveSignature : MenuCommandBase
    {
        private ISignatureOptions options;

        public RemoveSignature(IManipulator manipulator, ISignatureOptions options) 
        { 
            Manipulator = manipulator;
            this.options = options;
        }

        public override void RunCommand(string[] parameters)
        {
            if (parameters.Length != 2)
                return;

            string filePath = parameters[1];
            Manipulator.RemoveSignature(filePath, new DirectoryInfo(Path.GetDirectoryName(filePath)),
                options.Suffix);
        }
    }

    internal class ExtractDocument : MenuCommandExtractBase
    {
        public ExtractDocument(IManipulator manipulator) { Manipulator = manipulator; }

        public override void RunCommand(string[] parameters)
        {
            if (parameters.Length < 2 || parameters.Length > 3)
                return;

            string filePath = parameters[1];

            string dir = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(filePath),
                Path.GetFileNameWithoutExtension(filePath) + Resources.DefaultValues.DefaultValues.UnsignedSuffix)).FullName;

            IList<ILeveledBookmark> ranges;

            if (parameters.Length == 2)
                ranges = GetBookmarks(filePath);
            else
                ranges = GetBookmarks(filePath, parameters[2]);

            Manipulator.Extract(filePath, new DirectoryInfo(dir), ranges);
        }
    }
    internal class ExtractDirectory : MenuCommandExtractBase
    {
        public ExtractDirectory(IManipulator manipulator) { Manipulator = manipulator; }

        public override void RunCommand(string[] parameters)
        {
            if (parameters.Length < 2 || parameters.Length > 3)
                return;

            string parentFolder = FolderSelection.SelectFolder();
            if (parentFolder == null)
                return;

            string directoryPath = parameters[1];

            foreach (string file in Directory.GetFiles(directoryPath, "*.pdf", SearchOption.AllDirectories))
            {
                string dir = Directory.CreateDirectory(Path.Combine(parentFolder,
                    Path.GetFileNameWithoutExtension(file) + Resources.DefaultValues.DefaultValues.UnsignedSuffix)).FullName;

                IList<ILeveledBookmark> ranges;
                if (parameters.Length == 2)
                    ranges = GetBookmarks(file);
                else
                    ranges = GetBookmarks(file, parameters[2]);

                Manipulator.Extract(file, new DirectoryInfo(dir), ranges);
            }
        }
    }

    public static class FolderSelection
    {
        public static string SelectFolder()
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.ShowNewFolderButton = true;

            if (browseDialog.ShowDialog() == DialogResult.Cancel)
                return null;
            else
                return browseDialog.SelectedPath;
        }

    }

}
