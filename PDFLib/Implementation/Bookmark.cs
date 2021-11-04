using PDFLib.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.Implementation
{
    public class Bookmark : Internal.ModelBase, IBookmark
    {
        public Guid Id { get; }
        public Guid ParentId { get; set; }

        public string Title { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public int Level { get; set; }

        public string Range
        {
            get
            {
                if (StartPage == EndPage)
                    return StartPage.ToString();
                else
                    return string.Format("{0}-{1}", StartPage.ToString(), EndPage.ToString());
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        public Bookmark(string title, int pageNum)
        {
            Title = title;
            StartPage = pageNum;
            Id = Guid.NewGuid();
        }
        public Bookmark(string title, int pageNum, Guid id)
            : this(title, pageNum)
        {
            Id = id;
        }
        public Bookmark(string title, Guid id)
        {
            Title = title;
            Id = id;
        }
    }
}
