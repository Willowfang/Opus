using CX.LoggingLib;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class BookmarkDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private int startPage;
        public int StartPage
        {
            get { return startPage; }
            set 
            { 
                SetProperty(ref startPage, value);
                RaisePropertyChanged(nameof(EndPage));
            }
        }
        private int endPage;
        public int EndPage
        {
            get { return endPage; }
            set 
            { 
                SetProperty(ref endPage, value);
            }
        }
        private string? title;
        public string? Title
        {
            get { return title; }
            set 
            { 
                SetProperty(ref title, value);
            }
        }

        public BookmarkDialog(string dialogTitle)
            : base(dialogTitle) { }

        public string? Error
        {
            get => null;
        }

        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(StartPage))
                {
                    if (StartPage < 1)
                    {
                        return Resources.Validation.General.PageZero;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else if (propertyName == nameof(EndPage))
                {
                    if (EndPage < 1)
                    {
                        return Resources.Validation.General.PageZero;
                    }
                    else if (EndPage < StartPage)
                    {
                        return Resources.Validation.General.PageZero;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else if (propertyName == nameof(Title))
                {
                    if (string.IsNullOrEmpty(Title))
                    {
                        return Resources.Validation.General.NameEmpty;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
