namespace Opus.Common.Services.Navigation
{
    /// <summary>
    /// Registry for all viewmodels that have been registered as navigation targets.
    /// </summary>
    public interface INavigationTargetRegistry
    {
        /// <summary>
        /// Add a target to the registry.
        /// </summary>
        /// <param name="schemeName">Associate the target with this schemename.</param>
        /// <param name="target">Target viewModel.</param>
        public void AddTarget(string schemeName, INavigationTarget target);
    }
}
