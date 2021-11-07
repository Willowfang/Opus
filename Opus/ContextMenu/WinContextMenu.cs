using Opus.Services.Configuration;
using Opus.Core.ServiceImplementations.Configuration;
using Opus.Core.ServiceImplementations.Data;
using PDFLib.Services;
using System.IO;
using System.Windows.Forms;

namespace Opus.ContextMenu
{
    public interface IContextMenuCommand
    {
        public void RunCommand(string[] parameters);
    }

    public class RemoveSignature : IContextMenuCommand
    {
        private ISignature Signature;
        private IConfiguration.Sign Configuration;

        private RemoveSignature(ISignature signature, IConfiguration.Sign conf) 
        { 
            Signature = signature;
            Configuration = conf;
        }
        public static IContextMenuCommand GetService(ISignature signature, IConfiguration.Sign configuration) 
            => new RemoveSignature(signature, configuration);

        public void RunCommand(string[] parameters)
        {
            if (parameters.Length != 2)
                return;

            string filePath = parameters[1];
            Signature.RemoveCMD(filePath, Configuration.SignatureRemovePostfix);
        }
    }

    public class ExtractDocument : IContextMenuCommand
    {
        private IExtraction Extraction;

        private ExtractDocument(IExtraction extractionService) { Extraction = extractionService; }
        public static IContextMenuCommand GetService(IExtraction extractionService) 
            => new ExtractDocument(extractionService);

        public void RunCommand(string[] parameters)
        {
            if (parameters.Length < 2 || parameters.Length > 3)
                return;

            string filePath = parameters[1];

            string dir = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(filePath),
                Path.GetFileNameWithoutExtension(filePath) + Resources.Postfixes.Split)).FullName;

            if (parameters.Length == 2)
                Extraction.ExtractCMD(filePath, dir);
            else
                Extraction.ExtractCMD(filePath, dir, parameters[2]);
        }
    }
    public class ExtractDirectory : IContextMenuCommand
    {
        private IExtraction Extraction;

        private ExtractDirectory(IExtraction extractionService) { Extraction = extractionService; }
        public static IContextMenuCommand GetService(IExtraction extractionService)
            => new ExtractDirectory(extractionService);

        public void RunCommand(string[] parameters)
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

                if (parameters.Length == 2)
                    Extraction.ExtractCMD(file, dir);
                else
                    Extraction.ExtractCMD(file, dir, parameters[2]);
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
