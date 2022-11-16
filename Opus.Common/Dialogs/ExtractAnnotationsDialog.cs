using Opus.Common.Services.Dialogs;

namespace Opus.Common.Dialogs
{
    /// <summary>
    /// A class for holding information about a creator of an annotation.
    /// </summary>
    public class AnnotationCreator
    {
        /// <summary>
        /// Name of the creator.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Is this creator selected?
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Create a new annotation creator info.
        /// </summary>
        /// <param name="name">Name of the creator.</param>
        public AnnotationCreator(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// A dialog for enabling the user to select the annotations that will be removed.
    /// </summary>
    public class ExtractAnnotationsDialog : DialogBase, IDialog
    {
        /// <summary>
        /// A list of all the creators of annotations in all selected documents.
        /// </summary>
        public List<AnnotationCreator> Creators { get; }

        /// <summary>
        /// Create a new dialog for choosing creators, whose annotations will be removed.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="creatorNames">Names of the creators.</param>
        public ExtractAnnotationsDialog(string title, IEnumerable<string> creatorNames)
            : base(title)
        {
            Creators = new List<AnnotationCreator>();
            foreach (string name in creatorNames)
            {
                Creators.Add(new AnnotationCreator(name));
            }
        }

        /// <summary>
        /// Return the names of the creators that were selected by the user.
        /// </summary>
        /// <returns></returns>
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
