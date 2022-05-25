using CX.LoggingLib;
using CX.TaskManagerLib;
using Opus.Core.Base;
using Opus.Services.Configuration;
using Opus.Services.UI;
using Opus.Values;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Opus.Modules.Action.ViewModels
{
    public class TaskManageViewModel : ViewModelBaseLogging<TaskManageViewModel>, INavigationTarget
    {
        private readonly ITaskManager taskManager;
        private readonly IDialogAssist dialogAssist;

        private ObservableCollection<TaskItem> taskItems;
        public ObservableCollection<TaskItem> TaskItems
        {
            get => taskItems;
            set => SetProperty(ref taskItems, value);
        }

        private TaskItem selectedItem;
        public TaskItem SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

        public TaskManageViewModel(ITaskManager taskManager,
            IDialogAssist dialogAssist,
            INavigationTargetRegistry targetRegistry,
            ILogbook logbook)
            : base(logbook)
        {
            this.taskManager = taskManager;
            this.dialogAssist = dialogAssist;
            targetRegistry.AddTarget(SchemeNames.TASKS, this);
        }

        public async void OnArrival()
        {
            taskManager.PollingEvent += HandlePollingResults;
            await taskManager.Poll();
        }

        private void HandlePollingResults(object sender, TaskPollingEventArgs e)
        {
            TaskItems = new ObservableCollection<TaskItem>(e.TaskItems);
        }

        public void WhenLeaving()
        {
            taskManager.PollingEvent -= HandlePollingResults;
            taskManager.StopPolling();
        }
    }
}
