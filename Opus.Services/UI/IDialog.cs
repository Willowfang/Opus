using System.Threading.Tasks;
using System.Windows.Input;

namespace Opus.Services.UI
{
    public interface IDialog
    {
        public string DialogTitle { get; }
        public bool IsCanceled { get; }
        public TaskCompletionSource DialogClosed { get; set; }
        public ICommand Save { get; }
        public ICommand Close { get; }

        public void CloseOnError();
    }
}
