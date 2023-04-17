using Opus.Actions.Services.Redact;
using Opus.Common.Dialogs;
using Opus.Common.Logging;
using Opus.Common.Progress;
using Opus.Common.Services.Configuration;
using Opus.Common.Services.Dialogs;
using Opus.Common.Services.Input;
using Opus.Common.Wrappers;
using System.Windows;
using WF.LoggingLib;
using WF.PdfLib.Common.Redaction;
using WF.PdfLib.Services;

namespace Opus.Actions.Implementation.Redact
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class RedactMethods : LoggingCapable<RedactMethods>, IRedactMethods
    {
        private readonly IDialogAssist dialogAssist;
        private readonly IRedactProperties properties;
        private readonly IPathSelection pathSelection;
        private readonly IAnnotationService annotationService;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Create new instance for methods relating to redaction.
        /// </summary>
        /// <param name="dialogAssist">Dialog service.</param>
        /// <param name="properties">Shared redaction properties.</param>
        /// <param name="pathSelection">Path selection service.</param>
        /// <param name="annotationService">Service for manipulating annotations.</param>
        /// <param name="configuration">Application configurations.</param>
        /// <param name="logbook">Logging service.</param>
        public RedactMethods(
            IDialogAssist dialogAssist, 
            IRedactProperties properties, 
            IPathSelection pathSelection, 
            IAnnotationService annotationService, 
            IConfiguration configuration,
            ILogbook logbook) : base(logbook) 
        {
            this.dialogAssist = dialogAssist;
            this.properties = properties;
            this.pathSelection = pathSelection;
            this.annotationService = annotationService;
            this.configuration = configuration;
        }

        /// <summary>
        /// Delete a file from the list.
        /// </summary>
        public void ExecuteDelete()
        {
            if (properties.SelectedFile != null)
            {
                logbook.Write("Deleting file from list.", LogLevel.Information);

                properties.Files.Remove(properties.SelectedFile);
                properties.SelectedFile = null;

                logbook.Write("File deleted.", LogLevel.Information);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ApplyRedactions()
        {
            logbook.Write("Applying redactions (without text search).", LogLevel.Information);

            if (properties.Files.Count == 0)
            {
                logbook.Write("No files to redact.", LogLevel.Warning);
                return;
            }

            string? path = pathSelection.OpenDirectory(
                Resources.UserInput.Descriptions.SelectSaveFolder);

            if (string.IsNullOrEmpty(path))
            {
                logbook.Write($"Selected file path was null or empty.", LogLevel.Warning);
                return;
            }

            await ApplyAll(path);

            logbook.Write("Redactions applied.", LogLevel.Information);
        }

        private async Task ApplyAll(string directory)
        {
            ProgressTracker progress = new ProgressTracker(properties.Files.Count, dialogAssist);

            foreach (FileStorage file in properties.Files)
            {
                string output = GetOutputFilePath(directory, file);
                await annotationService.ApplyRedactions(file.FilePath, output, progress.Token);
                progress.Update(1);
            }

            progress.SetToComplete();
            await progress.Show;
        }

        private string GetOutputFilePath(string directory, FileStorage file)
        {
            string suffix = configuration.RedactFileSuffix ?? Resources.DefaultValues.DefaultValues.RedactSuffix;
            string filename = file.Title + suffix + Resources.Files.FileExtensions.Pdf;
            return Path.Combine(directory, filename);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteRedactions()
        {
            logbook.Write("Executing redactions.", LogLevel.Information);

            if (properties.Files.Count == 0)
            {
                logbook.Write("No files to redact.", LogLevel.Warning);
                return;
            }

            string? path = pathSelection.OpenDirectory(
                Resources.UserInput.Descriptions.SelectSaveFolder);

            if (string.IsNullOrEmpty(path))
            {
                logbook.Write($"Selected file path was null or empty.", LogLevel.Warning);
                return;
            }

            await ExecuteAll(path);

            logbook.Write("Redaction execution complete.", LogLevel.Information);
        }

        private async Task ExecuteAll(string directory)
        {
            List<IRedactionOption> options = GetAllOptions();

            if (options.Count == 0) 
            {
                MessageDialog dialog = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.Redaction.DefinitionsMissing);

                await dialogAssist.Show(dialog);

                return;
            }

            ProgressTracker progress = new ProgressTracker(properties.Files.Count, dialogAssist);

            foreach (FileStorage file in properties.Files)
            {
                string output = GetOutputFilePath(directory, file);
                await annotationService.CreateRedactions(
                    file.FilePath,
                    output,
                    progress.Token,
                    options);
                progress.Update(1);
            }

            progress.SetToComplete();
            await progress.Show;
        }

        private List<IRedactionOption> GetAllOptions() 
        {
            List<IRedactionOption> options = new List<IRedactionOption>();

            if (configuration.RedactRange)
            {
                IRedactionOption? range = GetRangeOption();
                if (range != null) options.Add(range);
            }

            if (configuration.RedactWords)
            {
                options.AddRange(GetWordOptions());
            }

            return options;
        }

        private List<IRedactionOption> GetWordOptions()
        {
            logbook.Write("Retrieving word options.", LogLevel.Debug);

            List<IRedactionOption> options = new List<IRedactionOption>();

            if (string.IsNullOrEmpty(properties.WordsToRedact))
            {
                logbook.Write("No words to redact.", LogLevel.Debug);
                return options;
            }

            string[] words = properties.WordsToRedact.Split(",");

            foreach (string word in words) 
            {
                options.Add(new RedactionWordOption(
                    word.Trim(),
                    configuration.RedactExecuteApply,
                    new RedactColor(configuration.RedactOutline),
                    new RedactColor(configuration.RedactFill)));
            }

            logbook.Write("Word options retrieved.", LogLevel.Debug);

            return options;
        }

        private IRedactionOption? GetRangeOption()
        {
            logbook.Write("Retrieving range option.", LogLevel.Debug);

            if (string.IsNullOrEmpty(configuration.RedactStart)
                || string.IsNullOrEmpty(configuration.RedactEnd)) 
            {
                logbook.Write("Both ranges have not been defined. Returning null option.", LogLevel.Debug);
                return null;
            }

            logbook.Write("Range option retrieved.", LogLevel.Debug);

            return new RedactionRangeOption(
                configuration.RedactStart,
                configuration.RedactEnd,
                configuration.RedactExecuteApply,
                new RedactColor(configuration.RedactOutline),
                new RedactColor(configuration.RedactFill));
        }
    }
}
