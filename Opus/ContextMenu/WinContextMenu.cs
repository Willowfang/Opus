using Opus.Services.Configuration;
using CX.PdfLib.Services;
using System.IO;
using System.Windows.Forms;
using CX.PdfLib.Services.Data;
using System.Collections.Generic;
using CX.PdfLib.Implementation.Data;

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
        protected IList<IExtractRange> GetRanges(string filePath)
        {
            List<IExtractRange> ranges = new List<IExtractRange>();
            foreach (ILeveledBookmark bookmark in Manipulator.FindBookmarks(filePath))
            {
                ranges.Add(new ExtractRange(bookmark.Title, bookmark.Pages));
            }

            return ranges;
        }

        protected IList<IExtractRange> GetRanges(string filePath, string preFix)
        {
            List<IExtractRange> ranges = new List<IExtractRange>();
            foreach (ILeveledBookmark bookmark in Manipulator.FindBookmarks(filePath))
            {
                if (bookmark.Title.ToLower().StartsWith(preFix.ToLower()))
                {
                    ranges.Add(new ExtractRange(bookmark.Title, bookmark.Pages));
                }
            }

            return ranges;
        }
    }

    internal class RemoveSignature : MenuCommandBase
    {
        private IConfiguration.Sign Configuration;

        public RemoveSignature(IManipulator manipulator, IConfiguration.Sign conf) 
        { 
            Manipulator = manipulator;
            Configuration = conf;
        }

        public override void RunCommand(string[] parameters)
        {
            if (parameters.Length != 2)
                return;

            string filePath = parameters[1];
            Manipulator.RemoveSignature(filePath, new DirectoryInfo(Path.GetDirectoryName(filePath)),
                Configuration.SignatureRemovePostfix);
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
                Path.GetFileNameWithoutExtension(filePath) + Resources.Postfixes.Split)).FullName;

            IList<IExtractRange> ranges;

            if (parameters.Length == 2)
                ranges = GetRanges(filePath);
            else
                ranges = GetRanges(filePath, parameters[2]);

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
                    Path.GetFileNameWithoutExtension(file) + Resources.Postfixes.Split)).FullName;

                IList<IExtractRange> ranges;
                if (parameters.Length == 2)
                    ranges = GetRanges(file);
                else
                    ranges = GetRanges(file, parameters[2]);

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
