using Opus.Services.Data.Composition;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// For comments, see <see cref="ICompositionSegment"/>
    /// </summary>
    public class CompositionFile : CompositionSegment, ICompositionFile
    {
        public override string? StructureName
        {
            get
            {
                if (NameFromFile)
                    return Resources.Labels.Composition.Descriptions.NameFromFile;

                return SegmentName;
            }
        }
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
        public override string? SegmentName
        {
            get => segmentName;
            set
            {
                SetProperty(ref segmentName, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }
        public bool NameFromFile { get; set; }

        private Regex? searchExpression;
        public Regex? SearchExpression
        {
            get => searchExpression;
        }
        private string? searchExpressionString;
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
        public Regex? IgnoreExpression
        {
            get => ignoreExpression;
        }
        private string? ignoreExpressionString;
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

        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public string? Example { get; set; }

        public CompositionFile() { }
        public CompositionFile(string segmentName)
        {
            SegmentName = segmentName;
            MinCount = 1;
            MaxCount = 0;
            Level = 1;
        }

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
    }
}
