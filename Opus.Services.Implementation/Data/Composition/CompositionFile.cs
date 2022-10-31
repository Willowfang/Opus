using Opus.Services.Data.Composition;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// Composition segment representing a file.
    /// <para>
    /// For more comments, see <see cref="ICompositionSegment"/>.
    /// </para>
    /// </summary>
    public class CompositionFile : CompositionSegment, ICompositionFile
    {
        #region Fields and properties
        /// <summary>
        /// Name of the composition segment structure.
        /// </summary>
        public override string? StructureName
        {
            get
            {
                if (NameFromFile)
                    return Resources.Labels.Composition.Descriptions.NameFromFile;

                return SegmentName;
            }
        }

        /// <summary>
        /// Name of the segment in a user-friendly form.
        /// </summary>
        public override string? DisplayName
        {
            get
            {
                if (MinCount == 0 && SegmentName != null)
                    return $"({SegmentName})";

                return SegmentName;
            }
        }
        private string? segmentName;

        /// <summary>
        /// Name of the segment, not necessarily in a user-friendly form (for that,
        /// check out <see cref="DisplayName"/>).
        /// </summary>
        public override string? SegmentName
        {
            get => segmentName;
            set
            {
                SetProperty(ref segmentName, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        /// <summary>
        /// Pull the final bookmark name from the filename.
        /// </summary>
        public bool NameFromFile { get; set; }

        private Regex? searchExpression;

        /// <summary>
        /// Regex to use for searching the correct files.
        /// </summary>
        public Regex? SearchExpression
        {
            get => searchExpression;
        }

        private string? searchExpressionString;

        /// <summary>
        /// Regex search expression in a string form (for displaying to the user and converting int regex
        /// from user input).
        /// </summary>
        public string? SearchExpressionString
        {
            get => searchExpressionString;
            set
            {
                searchExpressionString = value;
                if (value != null)
                    searchExpression = new Regex(value, RegexOptions.Compiled);
                else
                    searchExpression = null;
            }
        }

        private Regex? ignoreExpression;

        /// <summary>
        /// Part of the names to ignore when searching (in regex).
        /// </summary>
        public Regex? IgnoreExpression
        {
            get => ignoreExpression;
        }

        private string? ignoreExpressionString;

        /// <summary>
        /// Ignore regex as string presentation (for display and for converting from user input to regex).
        /// </summary>
        public string? IgnoreExpressionString
        {
            get => ignoreExpressionString;
            set
            {
                ignoreExpressionString = value;
                if (value != null)
                    ignoreExpression = new Regex(value, RegexOptions.Compiled);
                else
                    ignoreExpression = null;
            }
        }

        /// <summary>
        /// Minimun accepted amount of files for this segment.
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// Maximun accepted amount of files for this segment.
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// Example name to display to the user.
        /// </summary>
        public string? Example { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new Composition file segment.
        /// </summary>
        public CompositionFile() { }

        /// <summary>
        /// Create a new Composition file segment with a given name.
        /// </summary>
        /// <param name="segmentName">Name of the segment.</param>
        public CompositionFile(string segmentName)
        {
            SegmentName = segmentName;
            MinCount = 1;
            MaxCount = 0;
            Level = 1;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluate a file against the search expression and ignoring the ignore expression.
        /// </summary>
        /// <param name="filePath">Path of the file to check.</param>
        /// <returns>Evaluation result for the given file.</returns>
        /// <exception cref="NullReferenceException">Thrown, if search expression is null.</exception>
        public IFileEvaluationResult EvaluateFile(string filePath)
        {
            if (SearchExpression == null)
                throw new NullReferenceException(nameof(SearchExpression));

            string name = Path.GetFileNameWithoutExtension(filePath);
            if (IgnoreExpression != null)
            {
                name = IgnoreExpression.Replace(name, "");
            }
            if (SearchExpression.IsMatch(name))
            {
                return EvaluationResult.Match(filePath, name);
            }

            return EvaluationResult.NoMatch();
        }
        #endregion
    }
}
