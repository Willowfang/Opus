namespace Opus.Common.Services.Navigation
{
    /// <summary>
    /// Service for navigation targets of <see cref="INavigationAssist"/>
    /// </summary>
    public interface INavigationTarget
    {
        /// <summary>
        /// Operations to perform when navigated to by <see cref="INavigationAssist"/>
        /// </summary>
        public void OnArrival();
        /// <summary>
        /// Operations to perform when navigated from by <see cref="INavigationAssist"/>
        /// </summary>
        public void WhenLeaving();
        /// <summary>
        /// Operations to perform when a reset has been requested by the user.
        /// </summary>
        public void Reset();
    }
}
