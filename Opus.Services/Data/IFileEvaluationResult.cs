using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data
{
    public enum OutcomeType
    {
        Match,
        NoMatch
    }
    public interface IFileEvaluationResult 
    {
        public OutcomeType Outcome { get; }
        public string FilePath { get; }
        public string Name { get; }
    }
}
