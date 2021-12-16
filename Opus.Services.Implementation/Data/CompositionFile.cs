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
        public override string? DisplayName
        {
            get
            {
                string? name = segmentName;
                if (NameFromFile)
                {
                    name = Resources.Labels.Composition_NameFromFile;
                }
                if (MinCount > 0 && name != null)
                {
                    name = $"({name})";
                }
                return name;
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
        public Regex? SearchTerm { get; set; }
        public Regex? ToRemove { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }

        public CompositionFile()
        {
            MinCount = 1;
            MaxCount = 0;
        }
        public CompositionFile(string segmentName, bool getNameFromFile = false, 
            int minCount = 1, int maxCount = 0, int level = 1)
        {
            SegmentName = segmentName;
            NameFromFile = getNameFromFile;
            MinCount = minCount;
            MaxCount = maxCount;
            Level = level;
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
