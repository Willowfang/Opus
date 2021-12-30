using Opus.Services.Data;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Opus.Services.Implementation.Data
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

        public Regex? SearchTerm { get; private set; }
        public void SetSearchTerm(string regex)
        {
            SearchTerm = new Regex(regex, RegexOptions.Compiled);
        }
        public Regex? ToRemove { get; private set; }
        public void SetToRemove(string regex)
        {
            ToRemove = new Regex(regex, RegexOptions.Compiled);
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
            if (SearchTerm == null)
                throw new NullReferenceException(nameof(SearchTerm));

            string name = Path.GetFileNameWithoutExtension(filePath);
            if (ToRemove != null)
            {
                name = ToRemove.Replace(name, "");
            }
            if (SearchTerm.IsMatch(name))
            {
                return EvaluationResult.Match(filePath, name);
            }

            return EvaluationResult.NoMatch();
        }
    }
}
