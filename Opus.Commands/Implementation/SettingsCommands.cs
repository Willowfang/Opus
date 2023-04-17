using AsyncAwaitBestPractices.MVVM;
using Opus.Common.Services.Commands;
using Opus.Common.Services.Configuration;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using Opus.Common.Services.Dialogs;
using System.Windows.Input;
using WF.LoggingLib;
using System.Windows.Media;
using Opus.Common.Extensions;

namespace Opus.Commands.Implementation
{
    /// <summary>
    /// Commands related to settings dialogs.
    /// </summary>
    public abstract class SettingsCommandsBase<DialogType, SettingsType> : LoggingCapable<SettingsType>,
        ISettingsCommands where DialogType : IDialog
    {
        protected IDialogAssist dialogAssist;
        protected IConfiguration configuration;

        public SettingsCommandsBase(
            IDialogAssist dialogAssist,
            ILogbook logbook,
            IConfiguration configuration) : base(logbook)
        {
            this.dialogAssist = dialogAssist;
            this.configuration = configuration;
        }

        private IAsyncCommand? settingsCommand;

        /// <summary>
        /// Command for opening the settings dialog.
        /// </summary>
        public ICommand SettingsCommand =>
            settingsCommand ??= new AsyncCommand(ExecuteSettingsCommand);

        /// <summary>
        /// Execution method for settings opening command.
        /// <para>
        /// Creates the dialog using <see cref="CreateDialog"/> and displays it to the user.
        /// Saves the settings using <see cref="SaveSettings(DialogType)"/> if the dialog has not been canceled.
        /// </para>
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ExecuteSettingsCommand()
        {
            logbook.Write($"Displaying settings.", LogLevel.Information);

            DialogType dialog = CreateDialog();

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write($"Settings edit was cancelled.", LogLevel.Information);

                return;
            }

            logbook.Write($"Settings saved.", LogLevel.Information);

            SaveSettings(dialog);
        }

        /// <summary>
        /// Dialog creation method. Must be overridden for each implementation of this
        /// abstract class.
        /// </summary>
        /// <returns></returns>
        protected abstract DialogType CreateDialog();

        /// <summary>
        /// Method for saving the selected settings. Must be overridden for each implementation
        /// of this abstract class.
        /// </summary>
        /// <param name="dialog">The type of the dialog used for asking the new settings from the user.</param>
        protected abstract void SaveSettings(DialogType dialog);
    }

    public class CompositionSettingsCommands :
        SettingsCommandsBase<CompositionSettingsDialog, CompositionSettingsCommands>,
        IComposeSettingsCommands
    {
        public CompositionSettingsCommands(
            IDialogAssist dialogAssist,
            ILogbook logbook,
            IConfiguration configuration) : base(dialogAssist, logbook, configuration)
        {

        }

        /// <summary>
        /// Create the options dialog.
        /// <para>
        /// Dialog is <see cref="CompositionSettingsDialog"/>.
        /// </para>
        /// </summary>
        /// <returns>The created dialog.</returns>
        protected override CompositionSettingsDialog CreateDialog()
        {
            return new CompositionSettingsDialog(Resources.Labels.General.Settings)
            {
                SearchSubDirectories = configuration.CompositionSearchSubDirectories,
                DeleteConverted = configuration.CompositionDeleteConverted
            };
        }

        /// <summary>
        /// Save the modified options.
        /// </summary>
        /// <param name="dialog">Dialog where the options are held.</param>
        protected override void SaveSettings(CompositionSettingsDialog dialog)
        {
            configuration.CompositionSearchSubDirectories = dialog.SearchSubDirectories;
            configuration.CompositionDeleteConverted = dialog.DeleteConverted;
        }
    }

    public class ExtractSettingsCommands :
        SettingsCommandsBase<ExtractSettingsDialog, ExtractSettingsCommands>,
        IExtractSettingsCommands
    {
        public ExtractSettingsCommands(
            IDialogAssist dialogAssist,
            ILogbook logbook,
            IConfiguration configuration) : base(dialogAssist, logbook, configuration)
        {

        }

        /// <summary>
        /// Create a dialog for modifying options.
        /// </summary>
        /// <returns>The created dialog.</returns>
        protected override ExtractSettingsDialog CreateDialog()
        {
            return new ExtractSettingsDialog(Resources.Labels.General.Settings)
            {
                Title = configuration.ExtractionTitle,
                AlwaysAsk = configuration.ExtractionTitleAsk,
                PdfA = configuration.ExtractionConvertPdfA,
                PdfADisabled = configuration.ExtractionPdfADisabled,
                Annotations = configuration.Annotations,
                CreateZip = configuration.ExtractionCreateZip,
                GroupByFiles = configuration.GroupByFiles,
                OpenAfterComplete = configuration.ExtractionOpenDestinationAfterComplete
            };
        }

        /// <summary>
        /// Save modified options.
        /// </summary>
        /// <inheritdoc/>
        /// <param name="dialog"></param>
        protected override void SaveSettings(ExtractSettingsDialog dialog)
        {
            configuration.ExtractionTitle = dialog.Title!;
            configuration.ExtractionTitleAsk = dialog.AlwaysAsk;
            configuration.ExtractionConvertPdfA = dialog.PdfA;
            configuration.Annotations = dialog.Annotations;
            configuration.ExtractionCreateZip = dialog.CreateZip;
            configuration.GroupByFiles = dialog.GroupByFiles;
            configuration.ExtractionOpenDestinationAfterComplete = dialog.OpenAfterComplete;
        }
    }

    public class MergeSettingsCommands :
        SettingsCommandsBase<MergeSettingsDialog, MergeSettingsCommands>,
        IMergeSettingsCommands
    {
        public MergeSettingsCommands(
            IDialogAssist dialogAssist,
            ILogbook logbook,
            IConfiguration configuration) : base(dialogAssist, logbook, configuration)
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>Created dialog.</returns>
        protected override MergeSettingsDialog CreateDialog()
        {
            return new MergeSettingsDialog(Resources.Labels.General.Settings)
            {
                AddPageNumbers = configuration.MergeAddPageNumbers
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dialog">Dialog that contains the modified settings.</param>
        protected override void SaveSettings(MergeSettingsDialog dialog)
        {
            configuration.MergeAddPageNumbers = dialog.AddPageNumbers;
        }
    }

    public class WorkCopySettingsCommands :
        SettingsCommandsBase<WorkCopySettingsDialog, WorkCopySettingsCommands>,
        IWorkCopySettingsCommands
    {
        public WorkCopySettingsCommands(
            IDialogAssist dialogAssist,
            ILogbook logbook,
            IConfiguration configuration) : base(dialogAssist, logbook, configuration)
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>Created dialog.</returns>
        protected override WorkCopySettingsDialog CreateDialog()
        {
            string template =
                configuration.UnsignedTitleTemplate
                ?? Resources.DefaultValues.DefaultValues.UnsignedTemplate;
            return new WorkCopySettingsDialog(Resources.Labels.General.Settings)
            {
                TitleTemplate = template,
                FlattenRedactions = configuration.WorkCopyFlattenRedactions
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dialog">Dialog containing modified options.</param>
        protected override void SaveSettings(WorkCopySettingsDialog dialog)
        {
            configuration.UnsignedTitleTemplate = dialog.TitleTemplate;
            configuration.WorkCopyFlattenRedactions = dialog.FlattenRedactions;
        }
    }

    public class RedactSettingsCommands :
        SettingsCommandsBase<RedactSettingsDialog, RedactSettingsCommands>,
        IRedactSettingsCommands
    {
        public RedactSettingsCommands(
            IDialogAssist dialogAssist,
            ILogbook logbook,
            IConfiguration configuration) : base(dialogAssist, logbook, configuration) { }

        protected override RedactSettingsDialog CreateDialog()
        {
            RedactSettingsDialog dialog = new RedactSettingsDialog(
                Resources.Labels.General.Settings,
                configuration.Colors);
            dialog.SelectOutline(configuration.RedactOutline);
            dialog.SelectFill(configuration.RedactFill);
            dialog.Suffix = configuration.RedactFileSuffix ?? Resources.DefaultValues.DefaultValues.RedactSuffix;
            return dialog;
        }

        protected override void SaveSettings(RedactSettingsDialog dialog)
        {
            configuration.RedactOutline = dialog.SelectedOutline.BrushToHtmlHex();
            configuration.RedactFill = dialog.SelectedFill.BrushToHtmlHex();
            if (dialog.Suffix != Resources.DefaultValues.DefaultValues.RedactSuffix)
                configuration.RedactFileSuffix = dialog.Suffix;
        }
    }
}
