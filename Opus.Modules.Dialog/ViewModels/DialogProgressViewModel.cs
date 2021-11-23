using CX.PdfLib.Common;
using Opus.Core.Base;
using Opus.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Modules.Dialog.ViewModels
{
    public class DialogProgressViewModel : ViewModelBase
    {
        private int percent;
        public int Percent
        {
            get => percent;
            set => SetProperty(ref percent, value);
        }
        private string phase;
        public string Phase
        {
            get => phase;
            set => SetProperty(ref phase, value);
        }
        private string item;
        public string Item
        {
            get => item;
            set => SetProperty(ref item, value);
        }

        private bool showCloseButton;
        public bool ShowCloseButton
        {
            get => showCloseButton;
            set => SetProperty(ref showCloseButton, value);
        }

        public DialogProgressViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
            : base(regionManager, eventAggregator)
        {
            Aggregator.GetEvent<ProgressUpdateEvent>().Subscribe(ExecuteUpdateProgress);
        }

        private void ExecuteUpdateProgress(ProgressReport report)
        {
            Percent = report.Percentage;

            if (report.CurrentPhase != ProgressPhase.Finished)
            {
                Phase = GetPhaseName(report.CurrentPhase);
            }
            else
            {
                Phase = "Valmis!";
                Item = null;
                ShowCloseButton = true;
            }

            Item = report.CurrentItem;
        }

        private string GetPhaseName(ProgressPhase phase)
        {
            return phase switch
            {
                ProgressPhase.Unassigned => Resources.PhaseNames.Unassigned,
                ProgressPhase.Extracting => Resources.PhaseNames.Extracting,
                ProgressPhase.AddingBookmarks => Resources.PhaseNames.AddingBookmarks,
                ProgressPhase.Finished => null,
                _ => throw new ArgumentOutOfRangeException("Phase not defined")
            };
        }
    }
}
