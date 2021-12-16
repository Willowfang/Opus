using CX.PdfLib.Services.Data;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Opus.Services.Implementation.UI
{
    public class ReorderCollection<T> : BindableBase, IReorderCollection<T> where T : ILeveledItem
    {
        private ObservableCollection<T> items;
        private bool isReordering;
        private T? selectedItem;

        public ObservableCollection<T> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }
        public bool IsReordering
        {
            get => isReordering;
            set => SetProperty(ref isReordering, value);
        }
        public T SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

        private ICommand? moveUp;
        public ICommand MoveUp => moveUp ??= new DelegateCommand(ExecuteMoveUp);

        private ICommand? moveDown;
        public ICommand MoveDown => moveDown ??= new DelegateCommand(ExecuteMoveDown);

        private ICommand? moveLeft;
        public ICommand MoveLeft => moveLeft ??= new DelegateCommand(ExecuteMoveLeft);

        private ICommand? moveRight;
        public ICommand MoveRight => moveRight ??= new DelegateCommand(ExecuteMoveRight);

        public ReorderCollection()
        {
            this.items = new ObservableCollection<T>();
        }
        public ReorderCollection(ObservableCollection<T> items)
        {
            this.items = items ?? new ObservableCollection<T>();
        }

        private void ExecuteMoveUp()
        {
            if (SelectedItem == null)
                return;
            int currentIndex = Items.IndexOf(SelectedItem);
            // If item is topmost, do not move up. If item is second, but not on the first
            // level, do not move up (topmost item must be at level 1).
            if (currentIndex == 0 || (currentIndex == 1 && SelectedItem.Level > 1))
                return;
            // If item is second and is on the first level, move to topmost.
            //if (MoveIfIndexAndLevel(currentIndex, SelectedItem.Level, 1, 1, 0, true))
            //    return;

            int moveIndex = currentIndex;
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                T nextItem = Items[i];
                if (nextItem.Level == SelectedItem.Level)
                {
                    moveIndex = i;
                    for (int j = i; j >= 0; j--)
                    {
                        T nextLeveled = Items[j];
                        if (nextLeveled.Level == SelectedItem.Level)
                        {
                            moveIndex = j;
                            break;
                        }
                    }
                    break;
                }
            }

            if (moveIndex != currentIndex)
            {
                Items.Move(currentIndex, moveIndex);
                currentIndex++;
                moveIndex++;
                while (currentIndex < Items.Count && 
                    Items[currentIndex].Level > SelectedItem.Level)
                {
                    Items.Move(currentIndex, moveIndex);
                    currentIndex++;
                    moveIndex++;
                }
            }
        }
        private void ExecuteMoveDown()
        {
            if (SelectedItem == null)
                return;
            int currentIndex = Items.IndexOf(SelectedItem);
            int last = Items.Count - 1;
            if (currentIndex == last)
                return;

            int moveIndex = currentIndex;
            for (int i = currentIndex + 1; i < Items.Count; i++)
            {
                T nextItem = Items[i];
                if (nextItem.Level == SelectedItem.Level)
                {
                    moveIndex = i;
                    for (int j = i + 1; j < Items.Count; j++)
                    {
                        T nextLeveled = Items[j];
                        if (nextLeveled.Level > SelectedItem.Level)
                        {
                            moveIndex = j;
                            continue;
                        }
                        if (nextLeveled.Level <= SelectedItem.Level)
                            break;
                    }
                    break;
                }
            }
            if (moveIndex != currentIndex)
            {
                Items.Move(currentIndex, moveIndex);
                while (Items[currentIndex].Level > SelectedItem.Level)
                {
                    Items.Move(currentIndex, moveIndex);
                }
            }
        }
        private void ExecuteMoveRight()
        {
            if (SelectedItem == null) return;
            int currentIndex = Items.IndexOf(SelectedItem);
            if (currentIndex == 0) return;
            int previousLevel = Items[currentIndex - 1].Level;
            if ((SelectedItem.Level - previousLevel) <= 0)
                SelectedItem.Level += 1;
        }
        private void ExecuteMoveLeft()
        {
            if (SelectedItem == null) return;
            int currentIndex = Items.IndexOf(SelectedItem);
            bool isLast = false;
            if (currentIndex == Items.Count - 1)
                isLast = true;

            if (!isLast)
            {
                int nextLevel = Items[currentIndex + 1].Level;
                if ((nextLevel - SelectedItem.Level) <= 0 && SelectedItem.Level > 1)
                    SelectedItem.Level -= 1;
            }
            else
            {
                if (SelectedItem.Level > 1)
                    SelectedItem.Level -= 1;
            }
        }
    }
}
