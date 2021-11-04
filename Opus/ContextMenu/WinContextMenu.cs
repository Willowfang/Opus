using Opus.Services.Configuration;
using Opus.Core.ServiceImplementations.Configuration;
using Opus.Core.ServiceImplementations.Data;
using PDFLib.Services;
using System.IO;
using Opus.Methods;

namespace Opus.ContextMenu
{
    public interface IContextMenuCommand
    {
        public void RunCommand(string parameter);
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

        public void RunCommand(string filePath)
        {
            filePath.Replace("\"", "");
            Signature.RemoveCMD(filePath, Configuration.SignatureRemovePostfix);
        }
    }

    public class ExtractAll : IContextMenuCommand
    {
        private IExtraction Extraction;

        private ExtractAll(IExtraction extractionService) { Extraction = extractionService; }
        public static IContextMenuCommand GetService(IExtraction extractionService) 
            => new ExtractAll(extractionService);

        public void RunCommand(string filePath)
        {
            string dir = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(filePath),
                Path.GetFileNameWithoutExtension(filePath) + Resources.Postfixes.Split)).FullName;

            Extraction.ExtractCMD(filePath, dir);
        }
    }
    public class ExtractAppendices : IContextMenuCommand
    {
        private IExtraction Extraction;

        private ExtractAppendices(IExtraction extractionService) { Extraction = extractionService; }
        public static IContextMenuCommand GetService(IExtraction extractionService) 
            => new ExtractAppendices(extractionService);

        public void RunCommand(string filePath)
        {
            string dir = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(filePath),
                Path.GetFileNameWithoutExtension(filePath) + Resources.Postfixes.SplitAppendices)).FullName;

            Extraction.ExtractCMD(filePath, dir, Resources.SearchTerms.Appendice);
        }
    }
    public class DirExtractAll : IContextMenuCommand
    {
        private IExtraction Extraction;

        private DirExtractAll(IExtraction extractionService) { Extraction = extractionService; }
        public static IContextMenuCommand GetService(IExtraction extractionService)
            => new DirExtractAll(extractionService);

        public void RunCommand(string directoryPath)
        {
            string parentFolder = FolderSelection.SelectFolder();
            if (parentFolder == null)
                return;

            foreach (string file in Directory.GetFiles(directoryPath, "*.pdf", SearchOption.AllDirectories))
            {
                string dir = Directory.CreateDirectory(Path.Combine(parentFolder,
                    Path.GetFileNameWithoutExtension(file) + Resources.Postfixes.Split)).FullName;
                Extraction.ExtractCMD(file, dir);
            }
        }
    }
    public class DirExtractAppendices : IContextMenuCommand
    {
        private IExtraction Extraction;

        private DirExtractAppendices(IExtraction extractionService) { Extraction = extractionService; }
        public static IContextMenuCommand GetService(IExtraction extractionService)
            => new DirExtractAppendices(extractionService);

        public void RunCommand(string directoryPath)
        {
            string parentFolder = FolderSelection.SelectFolder();
            if (parentFolder == null)
                return;

            foreach (string file in Directory.GetFiles(directoryPath, "*.pdf", SearchOption.AllDirectories))
            {
                string dir = Directory.CreateDirectory(Path.Combine(parentFolder,
                    Path.GetFileNameWithoutExtension(file) + Resources.Postfixes.SplitAppendices)).FullName;
                Extraction.ExtractCMD(file, dir, Resources.SearchTerms.Appendice);
            }
        }
    }
}
