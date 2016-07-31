using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Delayware.MessageHandlers.Behaviors {

    /// <summary>
    /// Return the given status code
    /// </summary>
    public class StatusCodeBehavior : IPoisonBehavior {

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public string Status { get; set; }

        /// <summary>
        /// Executes the behavior asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task&lt;System.Net.HttpStatusCode&gt;.</returns>
        public Task<HttpStatusCode> ExecuteBehaviorAsync(CancellationToken cancellationToken) =>
            Task.FromResult((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), Status));
    }
}
