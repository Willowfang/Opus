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
    public class CompositionFileSegmentDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private bool nameFromFile;
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
        public string? SegmentName
        {
            get => segmentName;
            set => SetProperty(ref segmentName, value);
        }

        private string? searchTerm;
        public string? SearchTerm
        {
            get => searchTerm;
            set 
            {
                SetProperty(ref searchTerm, value);
            }
        }

        private string? toRemove;
        public string? ToRemove
        {
            get => toRemove;
            set
            {
                SetProperty(ref toRemove, value);
            }
        }

        private int minCount;
        public int MinCount
        {
            get => minCount;
            set => SetProperty(ref minCount, value);
        }

        private int maxCount;
        public int MaxCount
        {
            get => maxCount;
            set => SetProperty(ref maxCount, value);
        }
        private string? example;
        public string? Example
        {
            get => example;
            set => SetProperty(ref example, value);
        }

        public CompositionFileSegmentDialog(string dialogTitle)
            : base(dialogTitle) { }

        public string? Error
        {
            get => null;
        }

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
                    catch(ArgumentException)
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
