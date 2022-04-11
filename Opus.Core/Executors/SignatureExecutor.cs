using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using Opus.Core.Wrappers;
using Opus.ExtensionMethods;
using Opus.Services.Configuration;
using Opus.Services.Implementation.Logging;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opus.Core.Executors
{
    public interface ISignatureExecutor
    {
        public Task<IList<FileInfo>> Remove(IEnumerable<FileStorage> files, DirectoryInfo destination,
            CancellationTokenSource tokenSource);
    }

    public class SignatureExecutor : LoggingCapable<SignatureExecutor>, ISignatureExecutor
    {
        private readonly IConfiguration configuration;
        private readonly IDialogAssist dialogAssist;
        private readonly ISigningService signingService;


        public SignatureExecutor(
            IConfiguration configuration, 
            ISigningService signingService,
            IDialogAssist dialogAssist, 
            ILogbook logbook) : base(logbook)
        {
            this.configuration = configuration;
            this.signingService = signingService;
            this.dialogAssist = dialogAssist;
        }

        public async Task<IList<FileInfo>> Remove(IEnumerable<FileStorage> files, DirectoryInfo destination,
            CancellationTokenSource tokenSource)
        {
            CancellationToken token = tokenSource.Token;

            IList<FileInfo> created = await CallSignatureRemoval(files.ToList(), destination, tokenSource, token);

            if (token.IsCancellationRequested)
            {
                logbook.Write($"Cancellation requested at {nameof(CancellationToken)} '{token.GetHashCode()}'.", LogLevel.Information);
                return null;
            }

            return created;
        }

        private async Task<IList<FileInfo>> CallSignatureRemoval(IList<FileStorage> files, DirectoryInfo dir, 
            CancellationTokenSource tokenSource, CancellationToken token)
        {
            string destinationTemplate = configuration.UnsignedTitleTemplate + Values.FilePaths.PDF_EXTENSION;

            List<Task> removalTasks = new List<Task>();
            List<FileInfo> createdFiles = new List<FileInfo>();

            for (int i = 0; i < files.Count(); i++)
            {
                string destinationName = destinationTemplate.ReplacePlaceholder(Placeholders.File,
                    Path.GetFileNameWithoutExtension(files[i].FilePath));
                destinationName = destinationName.ReplacePlaceholder(Placeholders.Number,
                    (i + 1).ToString());

                FileInfo finalDestination = new FileInfo(Path.Combine(dir.FullName, destinationName));
                createdFiles.Add(finalDestination);
                removalTasks.Add(signingService.RemoveSignature(new FileInfo(files[i].FilePath),
                    finalDestination, token));
            }

            Task allRemovals = null;

            try
            {
                allRemovals = Task.WhenAll(removalTasks);
                await allRemovals;
            }
            catch (Exception e)
            {
                logbook.Write($"Signature removal tasks encountered an error", LogLevel.Error, e.InnerException);
            }

            if (allRemovals.Exception != null)
            {
                foreach (Exception inner in allRemovals.Exception.InnerExceptions)
                {
                    File.Delete(inner.Message);
                }

                MessageDialog message = new MessageDialog(Resources.Labels.General.Error,
                    Resources.Messages.Signature.RemovalFailed);

                await dialogAssist.Show(message);

                tokenSource.Cancel();
            }

            return createdFiles;
        }
    }
}
