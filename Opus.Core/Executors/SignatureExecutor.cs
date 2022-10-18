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
    /// <summary>
    /// Service for removing digital signatures from pdf-documents.
    /// <para>
    /// Default implementation in the same namespace.
    /// </para>
    /// </summary>
    public interface ISignatureExecutor
    {
        /// <summary>
        /// Remove digital signatures from a pdf-files.
        /// </summary>
        /// <param name="files">Files to remove signatures from.</param>
        /// <param name="destination">Directory, where products should be saved.</param>
        /// <param name="tokenSource">Cancellation token.</param>
        /// <returns></returns>
        public Task<IList<FileInfo>> Remove(
            IEnumerable<FileStorage> files,
            DirectoryInfo destination,
            CancellationTokenSource tokenSource
        );
    }

    /// <summary>
    /// Default implementation for <see cref="ISignatureExecutor"/>.
    /// <para>
    /// Has logging capabilities.
    /// </para>
    /// </summary>
    public class SignatureExecutor : LoggingCapable<SignatureExecutor>, ISignatureExecutor
    {
        // DI services
        private readonly IConfiguration configuration;
        private readonly IDialogAssist dialogAssist;
        private readonly ISigningService signingService;

        /// <summary>
        /// Create a new executor for signature removal.
        /// </summary>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="signingService">Service for removing signatures.</param>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="logbook">Logging service.</param>
        public SignatureExecutor(
            IConfiguration configuration,
            ISigningService signingService,
            IDialogAssist dialogAssist,
            ILogbook logbook
        ) : base(logbook)
        {
            // Assign DI services
            this.configuration = configuration;
            this.signingService = signingService;
            this.dialogAssist = dialogAssist;
        }

        /// <summary>
        /// Remove signatures from pdf-files.
        /// <para>
        /// Implements <see cref="ISignatureExecutor.Remove(IEnumerable{FileStorage}, DirectoryInfo, CancellationTokenSource)"/>.
        /// </para>
        /// </summary>
        /// <param name="files">Files to remove signature from.</param>
        /// <param name="destination">Directory to save the products in.</param>
        /// <param name="tokenSource">Cancellation token.</param>
        /// <returns>An awaitable task. The task will return the paths of the files that were produced.</returns>
        public async Task<IList<FileInfo>> Remove(
            IEnumerable<FileStorage> files,
            DirectoryInfo destination,
            CancellationTokenSource tokenSource
        )
        {
            // Remove signatures from files.

            CancellationToken token = tokenSource.Token;

            IList<FileInfo> created = await CallSignatureRemoval(
                files.ToList(),
                destination,
                tokenSource,
                token
            );

            if (token.IsCancellationRequested)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(CancellationToken)} '{token.GetHashCode()}'.",
                    LogLevel.Information
                );
                return null;
            }

            return created;
        }

        /// <summary>
        /// Internal signature removal method.
        /// </summary>
        /// <param name="files">Files to remove the signature from.</param>
        /// <param name="dir">Directory to save the files into.</param>
        /// <param name="tokenSource">Cancellation token source (for cancelling).</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        private async Task<IList<FileInfo>> CallSignatureRemoval(
            IList<FileStorage> files,
            DirectoryInfo dir,
            CancellationTokenSource tokenSource,
            CancellationToken token
        )
        {
            // Get the name template for products.

            string destinationTemplate =
                configuration.UnsignedTitleTemplate + Values.FilePaths.PDF_EXTENSION;

            // Track every removal task and only continue when all tasks have finished.

            List<Task> removalTasks = new List<Task>();
            List<FileInfo> createdFiles = new List<FileInfo>();

            for (int i = 0; i < files.Count(); i++)
            {
                string destinationName = destinationTemplate.ReplacePlaceholder(
                    Placeholders.File,
                    Path.GetFileNameWithoutExtension(files[i].FilePath)
                );
                destinationName = destinationName.ReplacePlaceholder(
                    Placeholders.Number,
                    (i + 1).ToString()
                );

                FileInfo finalDestination = new FileInfo(
                    Path.Combine(dir.FullName, destinationName)
                );
                createdFiles.Add(finalDestination);
                removalTasks.Add(
                    signingService.RemoveSignature(
                        new FileInfo(files[i].FilePath),
                        finalDestination,
                        token
                    )
                );
            }

            Task allRemovals = null;

            try
            {
                allRemovals = Task.WhenAll(removalTasks);
                await allRemovals;
            }
            catch (Exception e)
            {
                logbook.Write(
                    $"Signature removal tasks encountered an error",
                    LogLevel.Error,
                    e.InnerException
                );
            }

            if (allRemovals.Exception != null)
            {
                foreach (Exception inner in allRemovals.Exception.InnerExceptions)
                {
                    File.Delete(inner.Message);
                }

                MessageDialog message = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.Signature.RemovalFailed
                );

                await dialogAssist.Show(message);

                tokenSource.Cancel();
            }

            // If all removal tasks finished successfully, return the paths of the created files.

            return createdFiles;
        }
    }
}
