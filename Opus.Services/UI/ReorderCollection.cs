using WF.PdfLib.Services.Data;
using Opus.Services.Base;
using Opus.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Opus.Services.UI
{
    #region HelperClasses
    /// <summary>
    /// A command base collection move commands.
    /// </summary>
    public class MoveCommand : ICommand
    {
        private Action execute;

        /// <summary>
        /// When can execute changes.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Create a new move command.
        /// </summary>
        /// <param name="execute">Action to execute when command is called.</param>
        public MoveCommand(Action execute)
        {
            this.execute = execute;
        }

        /// <summary>
        /// Can the command execute.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Call when execution requested.
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            execute();
        }
    }

    /// <summary>
    /// Event arguments sent when collection has been changed.
    /// </summary>
    public class CollectionReorderedEventArgs
    {
        /// <summary>
        /// Old index of the item that has changed.
        /// </summary>
        public int OldIndex { get; }

        /// <summary>
        /// New index of the item that has changed.
        /// </summary>
        public int NewIndex { get; }

        /// <summary>
        /// Old level of the item that has changed.
        /// </summary>
        public int OldLevel { get; }

        /// <summary>
        /// New level of the item that has changed.
        /// </summary>
        public int NewLevel { get; }

        /// <summary>
        /// Create new event arguments.
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        /// <param name="oldLevel"></param>
        /// <param name="newLevel"></param>
        public CollectionReorderedEventArgs(int oldIndex, int newIndex, int oldLevel, int newLevel)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// Arguments for when a new item has been added to the collection.
    /// </summary>
    /// <typeparam name="T">Type of the added item.</typeparam>
    public class CollectionItemAddedEventArgs<T>
    {
        /// <summary>
        /// Item that was added.
        /// </summary>
        public T Item { get; }

        /// <summary>
        /// Create new arguments.
        /// </summary>
        /// <param name="item">Item that was added.</param>
        public CollectionItemAddedEventArgs(T item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// Arguments for when the selected item in the collection has changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollectionSelectedItemChangedEventArgs<T> : CollectionItemAddedEventArgs<T>
    {
        /// <summary>
        /// Create new arguments.
        /// </summary>
        /// <param name="item">New selected item.</param>
        public CollectionSelectedItemChangedEventArgs(T item) : base(item) { }
    }
    #endregion

    /// <summary>
    /// A customized collection based on <see cref="ObservableCollection{T}"/>, that allows
    /// for rearranging the items and other novel features.
    /// </summary>
    /// <typeparam name="T">Type of the items in the collection.</typeparam>
    public class ReorderCollection<T> : ObservableCollection<T> where T : ILeveledItem
    {
        #region EventHandlers
        /// <summary>
        /// Occurs when the collection has been reordered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CollectionReorderedEventHandler(
            object sender,
            CollectionReorderedEventArgs e
        );

        /// <summary>
        /// Occurs when a new item has been added to the collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CollectionItemAddedEventHandler(
            object sender,
            CollectionItemAddedEventArgs<T> e
        );

        /// <summary>
        /// Occurs when the selected item of the collection has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CollectionSelectedItemChangedEventHandler(
            object sender,
            CollectionSelectedItemChangedEventArgs<T> e
        );

        /// <summary>
        /// Occurs when the collection has been reordered.
        /// </summary>
        public event CollectionReorderedEventHandler CollectionReordered;

        /// <summary>
        /// Occurs when an item is added to the collection (using <see cref="Push(T)"/>).
        /// </summary>
        public event CollectionItemAddedEventHandler CollectionItemAdded;

        /// <summary>
        /// Occures when currently selected item changes.
        /// </summary>
        public event CollectionSelectedItemChangedEventHandler CollectionSelectedItemChanged;
        #endregion

        #region Fields and properties
        private bool isReordering;

        private T selectedItem;

        private bool canReorder;

        /// <summary>
        /// If true, the items in the collection can be rearranged.
        /// </summary>
        public bool CanReorder
        {
            get => canReorder;
            set => SetProperty(ref canReorder, value);
        }

        /// <summary>
        /// If true, the items in the collection are currently being rearranged.
        /// </summary>
        public bool IsReordering
        {
            get => isReordering;
            set => SetProperty(ref isReordering, value);
        }

        /// <summary>
        /// The item that is currently selected.
        /// </summary>
        public T SelectedItem
        {
            get => selectedItem;
            set
            {
                SetProperty(ref selectedItem, value);
                CollectionSelectedItemChanged?.Invoke(
                    this,
                    new CollectionSelectedItemChangedEventArgs<T>(SelectedItem)
                );
            }
        }

        /// <summary>
        /// If true, children items' levels will be dropped to 1 when deleting a parent. Defaults to true.
        /// </summary>
        public bool DropChildrenLevelsWhenDeleting { get; set; } = true;
        #endregion

        #region Commands
        private ICommand moveUp;

        /// <summary>
        /// Command for moving an item up.
        /// </summary>
        public ICommand MoveUp => moveUp ??= new MoveCommand(ExecuteMoveUp);

        private ICommand moveDown;

        /// <summary>
        /// Command for moving an item down.
        /// </summary>
        public ICommand MoveDown => moveDown ??= new MoveCommand(ExecuteMoveDown);

        private ICommand moveLeft;

        /// <summary>
        /// Command for decreasing an items level.
        /// </summary>
        public ICommand MoveLeft => moveLeft ??= new MoveCommand(ExecuteMoveLeft);

        private ICommand moveRight;

        /// <summary>
        /// Command for increasing an items level.
        /// </summary>
        public ICommand MoveRight => moveRight ??= new MoveCommand(ExecuteMoveRight);
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize a new, empty collection.
        /// </summary>
        public ReorderCollection() { }

        /// <summary>
        /// Initialize a collection from an enumerable.
        /// </summary>
        /// <param name="items">Items to initialize the collection with.</param>
        public ReorderCollection(IEnumerable<T> items) : base(items)
        {
            CanReorder = true;
        }

        /// <summary>
        /// Initialize a collection from a list.
        /// </summary>
        /// <param name="items">Items to initialize the collection with.</param>
        public ReorderCollection(List<T> items) : base(items)
        {
            CanReorder = true;
        }
        #endregion

        #region Command execution methods
        /// <summary>
        /// Execute the move of an item up (decrease index).
        /// </summary>
        protected void ExecuteMoveUp()
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
                while (currentIndex < Count && this[currentIndex].Level > SelectedItem.Level)
                {
                    Move(currentIndex, moveIndex);
                    currentIndex++;
                    moveIndex++;
                }
            }

            CollectionReordered?.Invoke(
                this,
                new CollectionReorderedEventArgs(oldIndex, newIndex, oldLevel, newLevel)
            );
        }

        /// <summary>
        /// Execute the move of an item down (increase index).
        /// </summary>
        protected void ExecuteMoveDown()
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

            CollectionReordered?.Invoke(
                this,
                new CollectionReorderedEventArgs(oldIndex, newIndex, oldLevel, newLevel)
            );
        }

        /// <summary>
        /// Execute the move of an item right (increase level).
        /// </summary>
        protected void ExecuteMoveRight()
        {
            if (SelectedItem == null || !CanReorder)
                return;

            int oldLevel = SelectedItem.Level;

            int currentIndex = IndexOf(SelectedItem);
            if (currentIndex == 0)
                return;
            int previousLevel = this[currentIndex - 1].Level;
            if (SelectedItem.Level - previousLevel <= 0)
                SelectedItem.Level += 1;

            int newLevel = SelectedItem.Level;

            CollectionReordered?.Invoke(
                this,
                new CollectionReorderedEventArgs(currentIndex, currentIndex, oldLevel, newLevel)
            );
        }

        /// <summary>
        /// Execute the move of an item left (decrease level).
        /// </summary>
        protected void ExecuteMoveLeft()
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

            CollectionReordered?.Invoke(
                this,
                new CollectionReorderedEventArgs(currentIndex, currentIndex, oldLevel, newLevel)
            );
        }
        #endregion

        #region Methods
        /// <summary>
        /// Push an item to the end of the collection.
        /// <para>
        /// Fires <see cref="CollectionItemAdded"/> event.
        /// </para>
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <returns>Return this collection. Makes it possible to chain actions.</returns>
        public ReorderCollection<T> Push(T item)
        {
            Add(item);
            CollectionItemAdded?.Invoke(this, new CollectionItemAddedEventArgs<T>(item));
            return this;
        }

        /// <summary>
        /// Remove selected items from the collection. Set selected item to null.
        /// Adjust childrens' levels if applicable.
        /// </summary>
        public void RemoveSelected()
        {
            if (typeof(ISelectable).IsAssignableFrom(typeof(T)))
            {
                if (DropChildrenLevelsWhenDeleting)
                {
                    foreach (T item in this)
                    {
                        if ((item as ISelectable).IsSelected)
                        {
                            DropChildrenLevels(item);
                        }
                    }
                }

                this.RemoveAll(x => (x as ISelectable).IsSelected);
            }
            else
            {
                if (DropChildrenLevelsWhenDeleting)
                {
                    DropChildrenLevels(SelectedItem);
                }

                Remove(SelectedItem);
            }
              
            SelectedItem = default(T);
        }

        private void DropChildrenLevels(T item)
        {
            int delIndex = IndexOf(item);
            for (int i = delIndex + 1; i < Count; i++)
            {
                if (this[i].Level <= item.Level)
                {
                    break;
                }

                this[i].Level = 1;
            }
        }

        /// <summary>
        /// Set property for <see cref="INotifyPropertyChanged.PropertyChanged"/> implementation.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        protected void SetProperty<TValue>(
            ref TValue field,
            TValue value,
            [CallerMemberName] string propertyName = null
        )
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return;

            field = value;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
