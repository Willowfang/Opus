using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFExtractor.Core.Singletons
{
    public interface ICommonValues
    {
        public string Identifier { get; set; }
    }

    public class CommonValues : BindableBase, ICommonValues
    {
        private string identifier;
        public string Identifier
        {
            get => identifier;
            set => SetProperty(ref identifier, value);
        }
    }
}
