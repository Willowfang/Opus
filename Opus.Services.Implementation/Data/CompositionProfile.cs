using Opus.Services.Data;
using Opus.Services.UI;

namespace Opus.Services.Implementation.Data
{
    /// <summary>
    /// For comments, see <see cref="ICompositionProfile"/>
    /// </summary>
    public class CompositionProfile : ICompositionProfile
    {
        public int Id { get; set; }
        public string? ProfileName { get; set; }
        public IReorderCollection<ICompositionSegment> Segments { get; }
        public bool AddPageNumbers { get; set; }
        public bool IsEditable { get; set; }

        public CompositionProfile(IReorderCollection<ICompositionSegment> segments)
        {
            Segments = segments;
        }
    }
}
