using CX.LoggingLib;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// Dialog for creating a new file segment or editing an existing one.
    /// </summary>
    public class CompositionFileSegmentDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private bool nameFromFile;

        /// <summary>
        /// If true, the name of the bookmark will be taken from the file found.
        /// </summary>
        public bool NameFromFile
        {
            get => nameFromFile;
            set
            {
                SetProperty(ref nameFromFile, value);
                RaisePropertyChanged(nameof(SegmentName));
            }
        }

        private string? segmentName;

        /// <summary>
        /// Name of this segment.
        /// </summary>
        public string? SegmentName
        {
            get => segmentName;
            set => SetProperty(ref segmentName, value);
        }

        private string? searchTerm;

        /// <summary>
        /// Search term as a string.
        /// </summary>
        public string? SearchTerm
        {
            get => searchTerm;
            set { SetProperty(ref searchTerm, value); }
        }

        private string? toRemove;

        /// <summary>
        /// Part of the strings to ignore.
        /// </summary>
        public string? ToRemove
        {
            get => toRemove;
            set { SetProperty(ref toRemove, value); }
        }

        private int minCount;

        /// <summary>
        /// Minimun amount of matching files.
        /// </summary>
        public int MinCount
        {
            get => minCount;
            set => SetProperty(ref minCount, value);
        }

        private int maxCount;

        /// <summary>
        /// Maximun amount of matching files.
        /// </summary>
        public int MaxCount
        {
            get => maxCount;
            set => SetProperty(ref maxCount, value);
        }
        private string? example;

        /// <summary>
        /// An example name shown to the user.
        /// </summary>
        public string? Example
        {
            get => example;
            set => SetProperty(ref example, value);
        }

        /// <summary>
        /// Create a new dialog for getting file segment info from the user.
        /// </summary>
        /// <param name="dialogTitle">Title of this dialog.</param>
        public CompositionFileSegmentDialog(string dialogTitle) : base(dialogTitle) { }

        /// <summary>
        /// Validation error, always return null.
        /// </summary>
        public string? Error
        {
            get => null;
        }

        /// <summary>
        /// Validation.
        /// </summary>
        /// <param name="propertyName">Property to validate.</param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(SegmentName))
                {
                    if (string.IsNullOrEmpty(SegmentName))
                        return Resources.Validation.General.NameEmpty;
                }

                if (propertyName == nameof(SearchTerm))
                {
                    if (string.IsNullOrEmpty(SearchTerm))
                        return Resources.Validation.Composition.ExpressionEmpty;

                    try
                    {
                        new Regex(SearchTerm);
                    }
                    catch (ArgumentException)
                    {
                        return Resources.Validation.Composition.ExpressionInvalid;
                    }
                }

                if (propertyName == nameof(ToRemove))
                {
                    if (string.IsNullOrEmpty(ToRemove))
                        return string.Empty;

                    try
                    {
                        new Regex(ToRemove);
                    }
                    catch (ArgumentException)
                    {
                        return Resources.Validation.Composition.ExpressionInvalid;
                    }
                }

                return string.Empty;
            }
        }
    }
}
