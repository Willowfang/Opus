using Opus.Events;
using Opus.Services.UI;
using Prism.Events;
using Prism.Mvvm;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// View model for displaying instructions.
    /// </summary>
    public class InstructionsViewModel : BindableBase
    {
        private readonly ISchemeInstructions schemeInstructions;

        private string title;
        /// <summary>
        /// Instructions title.
        /// </summary>
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private Instruction[] instructions;
        /// <summary>
        /// Instructions shown as an array.
        /// </summary>
        public Instruction[] Instructions
        {
            get => instructions;
            set => SetProperty(ref instructions, value);
        }

        /// <summary>
        /// Create a new view model for displaying instructions.
        /// </summary>
        public InstructionsViewModel(
            IEventAggregator eventAggregator,
            ISchemeInstructions schemeInstructions)
        {
            eventAggregator.GetEvent<ViewChangeEvent>().Subscribe(SetInstructionsAndTitle);
            this.schemeInstructions = schemeInstructions;
        }

        private void SetInstructionsAndTitle(string schemeName)
        {
            schemeInstructions.SetScheme(schemeName);
            Instructions = schemeInstructions.Instructions();
            Title = schemeInstructions.Title();
        }
    }
}
