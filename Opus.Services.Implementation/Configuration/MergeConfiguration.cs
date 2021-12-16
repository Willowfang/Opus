using Opus.Services.Configuration;
using Opus.Services.Data;
using Opus.Services.Implementation.Data;

namespace Opus.Services.Implementation.Configuration
{
    public class MergeConfiguration : IConfiguration.Merge
    {
        private class MergeOptions : DataObject<MergeOptions>
        {
            public bool AddPageNumbers { get; set; }

            public MergeOptions()
            {
                AddPageNumbers = true;
            }

            public override int GetHashCode()
            {
                return AddPageNumbers.GetHashCode();
            }
            protected override bool CheckEquality(MergeOptions current, MergeOptions other)
            {
                return current.AddPageNumbers == other.AddPageNumbers;
            }
        }

        private IDataProvider provider;
        private MergeOptions options;

        public MergeConfiguration(IDataProvider provider)
        {
            this.provider = provider;

            var current = new MergeOptions();

            var found = provider.GetOneById<MergeOptions>(1);
            if (found == null) provider.Save(current);
            options = found ?? current;
        }

        public bool AddPageNumbers
        {
            get => options.AddPageNumbers;
            set
            {
                var current = provider.GetOneById<MergeOptions>(1);
                current.AddPageNumbers = value;
                provider.Save(current);
                options.AddPageNumbers = value;
            }
        }
    }
}
