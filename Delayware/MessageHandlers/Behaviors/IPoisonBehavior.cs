using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Delayware.MessageHandlers.Behaviors {
    /// <summary>
    /// IPoisonBehavior
    /// </summary>
    public interface IPoisonBehavior {

        /// <summary>
        /// Executes the behavior asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;HttpStatusCode&gt;.</returns>
        Task<HttpStatusCode> ExecuteBehaviorAsync(CancellationToken cancellationToken);
    }
}
