using Delayware.MessageHandlers.Behaviors;

namespace Delayware.MessageHandlers.Strategies {
    /// <summary>
    /// IPoisonStrategy
    /// </summary>
    public interface IPoisonStrategy : IPoisonBehavior {

        /// <summary>
        /// Setups the strategy.
        /// </summary>
        /// <param name="token">The token.</param>
        void SetupStrategy(string token);

    }
}
