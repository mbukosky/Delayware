using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Delayware.MessageHandlers.Behaviors;

namespace Delayware.MessageHandlers.Strategies {
    /// <summary>
    /// Default strategy for executing the poison pill
    /// </summary>
    public class DefaultPoisonStrategy : IPoisonStrategy, IDisposable {

        private readonly string _secret;
        private readonly MemoryCache _cache = new MemoryCache("DefaultPoisonStrategy");

        private IPoisonBehavior _currentBehavior;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPoisonStrategy"/> class.
        /// </summary>
        /// <param name="secret">The secret.</param>
        public DefaultPoisonStrategy(string secret) {
            _secret = secret;
        }

        /// <summary>
        /// Executes the behavior asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;HttpStatusCode&gt;.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<HttpStatusCode> ExecuteBehaviorAsync(CancellationToken cancellationToken) {
            var status = HttpStatusCode.OK;
            if (_currentBehavior != null) {
                status = await _currentBehavior.ExecuteBehaviorAsync(cancellationToken);
            } else if (_cache.Contains("timed")) {
                var delay = (IPoisonBehavior)_cache.Get("timed");
                status = await delay.ExecuteBehaviorAsync(cancellationToken);
            }

            // Always clear the current delay
            _currentBehavior = null;

            return status;
        }

        /// <summary>
        /// Setups the strategy.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void SetupStrategy(string token) {
            //TODO: JWT.SignatureVerificationException
            var payload = GetPayloadFromToken(token);

            //TODO: make this load via ioc or something
            string action, type;

            payload.TryGetValue("action", out action);
            payload.TryGetValue("type", out type);

            IPoisonBehavior behavior = null;

            // Select the behavior
            switch (type) {
                case "delay":
                    behavior = new DelayBehavior {
                        From = int.Parse(payload["from"], CultureInfo.InvariantCulture),
                        To = int.Parse(payload["to"], CultureInfo.InvariantCulture)
                    };
                    break;
                case "status":
                    behavior = new StatusCodeBehavior() {
                        Status = payload["code"]
                    };
                    break;
                case "clear":
                    _currentBehavior = null;
                    _cache.Remove("timed");
                    return;
                default:
                    // Just leave if we don't have the correct behavior
                    return;
            }

            // Select the action
            switch (action) {
                case "single":
                    _currentBehavior = behavior;
                    _cache.Remove("timed");
                    break;
                case "timed":
                    _currentBehavior = null;
                    _cache.Set("timed", behavior, new CacheItemPolicy {
                        AbsoluteExpiration = DateTime.UtcNow.AddSeconds(int.Parse(payload["duration"], CultureInfo.InvariantCulture))
                    });
                    break;
            }
        }

        private IDictionary<string, string> GetPayloadFromToken(string token) =>
            JWT.JsonWebToken.DecodeToObject<IDictionary<string, string>>(token, _secret);

        /// <summary>
        /// Disposes the specified disposing.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing)
                _cache.Dispose();
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
