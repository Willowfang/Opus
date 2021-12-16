using System.Threading.Tasks;
using System.Windows.Input;

namespace Opus.Services.UI
{
    public interface IDialog
    {
        public bool IsCanceled { get; }
        public TaskCompletionSource DialogClosed { get; set; }
        public ICommand Save { get; }
        public ICommand Close { get; }
    }
}
