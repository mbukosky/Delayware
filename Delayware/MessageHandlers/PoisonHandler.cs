using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Delayware.MessageHandlers.Strategies;

namespace Delayware.MessageHandlers {
    /// <summary>
    /// Class PoisonHandler.
    /// </summary>
    public class PoisonHandler : DelegatingHandler {
        private readonly IPoisonStrategy _strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoisonHandler"/> class.
        /// </summary>
        /// <param name="strategy">The delay.</param>
        public PoisonHandler(IPoisonStrategy strategy) {
            _strategy = strategy;
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task&lt;System.Net.Http.HttpResponseMessage&gt;.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {

            //Poison can be set on any public url
            IEnumerable<string> values;
            if (request.Headers.TryGetValues("X-POISON", out values)) {
                _strategy.SetupStrategy(values.FirstOrDefault());
            }

            //Behaviors should only take place on controllers
            if (request.RequestUri.LocalPath.StartsWith("/v1")) { //TODO: Make this configurable
                var status = await _strategy.ExecuteBehaviorAsync(cancellationToken);
                if (status != HttpStatusCode.OK)
                    return new HttpResponseMessage(status);
            }

            //Allow the request to process further down the pipeline and return the response back up the chain
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
