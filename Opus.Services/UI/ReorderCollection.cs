using CX.PdfLib.Services.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Opus.Services.UI
{
    public class MoveCommand : ICommand
    {
        private Action execute;
        public event EventHandler CanExecuteChanged;

        public MoveCommand(Action execute)
        {
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            execute();
        }
    }

    public class CollectionReorderedEventArgs
    {
        public int OldIndex { get; }
        public int NewIndex { get; }
        public int OldLevel { get; }
        public int NewLevel { get; }

        public CollectionReorderedEventArgs(int oldIndex, int newIndex,
            int oldLevel, int newLevel)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }

    public class CollectionItemAddedEventArgs<T>
    {
        public T Item { get; }

        public CollectionItemAddedEventArgs(T item)
        {
            Item = item;
        }
    }

    public class ReorderCollection<T> : ObservableCollection<T> where T : ILeveledItem
    {
        public delegate void CollectionReorderedEventHandler(object sender, CollectionReorderedEventArgs e);
        public delegate void CollectionItemAddedEventHandler(object sender, CollectionItemAddedEventArgs<T> e);
        /// <summary>
        /// Occurs when the collection has been reordered.
        /// </summary>
        public event CollectionReorderedEventHandler CollectionReordered;
        /// <summary>
        /// Occurs when an item is added to the collection (using <see cref="Push(T)"/>).
        /// </summary>
        public event CollectionItemAddedEventHandler CollectionItemAdded;

        private bool isReordering;
        private T selectedItem;

        private bool canReorder;
        public bool CanReorder
        {
            get => canReorder;
            set => SetProperty(ref canReorder, value);
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

        private ICommand moveUp;
        public ICommand MoveUp => moveUp ??= new MoveCommand(ExecuteMoveUp);

        private ICommand moveDown;
        public ICommand MoveDown => moveDown ??= new MoveCommand(ExecuteMoveDown);

        private ICommand moveLeft;
        public ICommand MoveLeft => moveLeft ??= new MoveCommand(ExecuteMoveLeft);

        private ICommand moveRight;
        public ICommand MoveRight => moveRight ??= new MoveCommand(ExecuteMoveRight);

        public ReorderCollection() { }
        public ReorderCollection(IEnumerable<T> items)
            : base(items) 
        {
            CanReorder = true;
        }
        public ReorderCollection(List<T> items)
            : base(items) 
        {
            CanReorder = true;
        }

        public ReorderCollection<T> Push(T item)
        {
            Add(item);
            CollectionItemAdded?.Invoke(this, new CollectionItemAddedEventArgs<T>(item));
            return this;
        }

        protected void SetProperty<TValue>(ref TValue field, TValue value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value)) return;

            field = value;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void ExecuteMoveUp()
        {
            if (SelectedItem == null || !CanReorder)
                return;

            int currentIndex = IndexOf(SelectedItem);

            // If item is topmost, do not move up. If item is second, but not on the first
            // level, do not move up (topmost item must be at level 1).
            if (currentIndex == 0 || currentIndex == 1 && SelectedItem.Level > 1)
                return;

            // If item is second and is on the first level, move to topmost.
            int moveIndex = currentIndex;
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                T nextItem = this[i];
                if (nextItem.Level == SelectedItem.Level)
                {
                    moveIndex = i;
                    for (int j = i; j >= 0; j--)
                    {
                        T nextLeveled = this[j];
                        if (nextLeveled.Level == SelectedItem.Level)
                        {
                            moveIndex = j;
                            break;
                        }
                    }
                    break;
                }
            }

            int oldIndex = currentIndex;
            int newIndex = moveIndex;
            int oldLevel = SelectedItem.Level;
            int newLevel = SelectedItem.Level;

            if (moveIndex != currentIndex)
            {
                Move(currentIndex, moveIndex);
                currentIndex++;
                moveIndex++;
                while (currentIndex < Count &&
                    this[currentIndex].Level > SelectedItem.Level)
                {
                    Move(currentIndex, moveIndex);
                    currentIndex++;
                    moveIndex++;
                }
            }

            CollectionReordered?.Invoke(this,
                new CollectionReorderedEventArgs(oldIndex, newIndex, oldLevel, newLevel));
        }
        private void ExecuteMoveDown()
        {
            if (SelectedItem == null || !CanReorder)
                return;

            int currentIndex = IndexOf(SelectedItem);
            int last = Count - 1;
            if (currentIndex == last)
                return;

            int moveIndex = currentIndex;
            for (int i = currentIndex + 1; i < Count; i++)
            {
                T nextItem = this[i];
                if (nextItem.Level == SelectedItem.Level)
                {
                    moveIndex = i;
                    for (int j = i + 1; j < Count; j++)
                    {
                        T nextLeveled = this[j];
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

            int oldIndex = currentIndex;
            int newIndex = moveIndex;
            int oldLevel = SelectedItem.Level;
            int newLevel = SelectedItem.Level;

            if (moveIndex != currentIndex)
            {
                Move(currentIndex, moveIndex);
                while (this[currentIndex].Level > SelectedItem.Level)
                {
                    Move(currentIndex, moveIndex);
                }
            }

            CollectionReordered?.Invoke(this,
                new CollectionReorderedEventArgs(oldIndex, newIndex, oldLevel, newLevel));
        }
        private void ExecuteMoveRight()
        {
            if (SelectedItem == null || !CanReorder) 
                return;

            int oldLevel = SelectedItem.Level;

            int currentIndex = IndexOf(SelectedItem);
            if (currentIndex == 0) return;
            int previousLevel = this[currentIndex - 1].Level;
            if (SelectedItem.Level - previousLevel <= 0)
                SelectedItem.Level += 1;

            int newLevel = SelectedItem.Level;

            CollectionReordered?.Invoke(this,
                new CollectionReorderedEventArgs(currentIndex, currentIndex, oldLevel, newLevel));
        }
        private void ExecuteMoveLeft()
        {
            if (SelectedItem == null || !CanReorder) 
                return;

            int oldLevel = SelectedItem.Level;

            int currentIndex = IndexOf(SelectedItem);
            bool isLast = false;
            if (currentIndex == Count - 1)
                isLast = true;

            if (!isLast)
            {
                int nextLevel = this[currentIndex + 1].Level;
                if (nextLevel - SelectedItem.Level <= 0 && SelectedItem.Level > 1)
                    SelectedItem.Level -= 1;
            }
            else
            {
                if (SelectedItem.Level > 1)
                    SelectedItem.Level -= 1;
            }

            int newLevel = SelectedItem.Level;

            CollectionReordered?.Invoke(this,
                new CollectionReorderedEventArgs(currentIndex, currentIndex, oldLevel, newLevel));
        }
    }
}
