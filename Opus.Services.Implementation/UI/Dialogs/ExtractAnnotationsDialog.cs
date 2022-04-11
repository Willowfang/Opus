using CX.PdfLib.Services;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class AnnotationCreator
    {
        public string Name { get; }
        public bool IsChecked { get; set; }

        public AnnotationCreator(string name)
        {
            Name = name;
        }
    }

    public class ExtractAnnotationsDialog : DialogBase, IDialog
    {
        public List<AnnotationCreator> Creators { get; }

        public ExtractAnnotationsDialog(string title, IEnumerable<string> creatorNames) : base(title)
        {
            Creators = new List<AnnotationCreator>();
            foreach (string name in creatorNames)
            {
                Creators.Add(new AnnotationCreator(name));
            }
        }

        public IEnumerable<string> GetSelectedCreators()
        {
            List<AnnotationCreator> selected = new List<AnnotationCreator>();
            foreach (AnnotationCreator creator in Creators)
            {
                if (creator.IsChecked)
                    selected.Add(creator);
            }
            List<string> names = new List<string>();
            foreach (AnnotationCreator creator in selected)
            {
                names.Add(creator.Name);
            }
            return names;
        }
    }
}
