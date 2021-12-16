using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opus.Services.UI
{
    public interface IReorderCollection<T>
    {
        public ObservableCollection<T> Items { get; }
        public bool IsReordering { get; }
        public T SelectedItem { get; set; }
        public ICommand MoveUp { get; }
        public ICommand MoveDown { get; }
        public ICommand MoveLeft { get; }
        public ICommand MoveRight { get; }
    }
}
