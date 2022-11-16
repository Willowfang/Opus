namespace Opus.Actions.Services.Base
{
    /// <summary>
    /// Base interface for event handling interfaces with navigation capabilities.
    /// </summary>
    public interface INavigationEventHandling : IActionEventHandling
    {
        /// <summary>
        /// Subscribe to events for this action.
        /// </summary>
        public void SubscribeToEvents();

        /// <summary>
        /// Unsubscribe from events of this action.
        /// </summary>
        public void UnsubscribeFromEvents();

        /// <summary>
        /// Reset operations for this action.
        /// </summary>
        public void ActionReset();

    }
}
