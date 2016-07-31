using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Delayware.MessageHandlers.Behaviors {

    /// <summary>
    /// Random delay within a range
    /// </summary>
    public class DelayBehavior : IPoisonBehavior {

        /// <summary>
        /// Gets or sets From in ms.
        /// </summary>
        /// <value>From.</value>
        public int From { get; set; }

        /// <summary>
        /// Gets or sets To in ms.
        /// </summary>
        /// <value>To.</value>
        public int To { get; set; }

        /// <summary>
        /// execute behavior as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;HttpStatusCode&gt;.</returns>
        public async Task<HttpStatusCode> ExecuteBehaviorAsync(CancellationToken cancellationToken) {
            var r = new Random(DateTime.Now.Millisecond);
            var delayMs = r.Next(From, To);

            // Delay all incoming requests
            await Task.Delay(delayMs, cancellationToken);

            return HttpStatusCode.OK;
        }
    }
}
