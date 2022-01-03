using Opus.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data.Composition
{
    public enum OutcomeType
    {
        Match,
        NoMatch
    }
    public interface IFileEvaluationResult : ISelectable
    {
        public OutcomeType Outcome { get; }
        public string FilePath { get; }
        public string Name { get; }
    }
}
