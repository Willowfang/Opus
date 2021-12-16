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
            get => identifier ?? configuration.SignatureRemovePostfix;
            set
            {
                configuration.SignatureRemovePostfix = value;
                SetProperty(ref identifier, value);
            }
        }

        public IdentifierViewModel(IConfiguration.Sign configuration)
        {
            this.configuration = configuration;
        }
    }
}
