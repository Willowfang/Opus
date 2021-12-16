using Opus.Services.Configuration;
using Opus.Services.Data;
using System.Collections.Generic;

namespace Opus.Services.Implementation.Configuration
{
    public class ComposeConfiguration : IConfiguration.Compose
    {
        private IDataProvider provider;

        public ComposeConfiguration(IDataProvider provider)
        {
            this.provider = provider;
        }

        public IList<ICompositionProfile> GetProfiles()
        {
            return provider.GetAll<ICompositionProfile>();
        }

        public ICompositionProfile SaveProfile(ICompositionProfile profile)
        {
            return provider.Save(profile);
        }
    }
}
