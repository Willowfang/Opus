using Opus.Core.Base;
using Opus.Services.Configuration;
using Prism.Events;
using Prism.Regions;

namespace Opus.Modules.Options.ViewModels
{
    public class IdentifierViewModel : ViewModelBase
    {
        private IConfiguration.Sign configuration;

        private string identifier;
        public string Identifier
        {
            get
            {
                return identifier ?? configuration.SignatureRemovePostfix;
            }
            set
            {
                configuration.SignatureRemovePostfix = value;
                identifier = value;
                RaisePropertyChanged();
            }
        }

        public IdentifierViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, 
            IConfiguration.Sign conf)
            : base(regionManager, eventAggregator)
        {
            configuration = conf;
        }
    }
}
