using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Delayware.UnitTests.MessageHandlers {
    //http://stackoverflow.com/questions/9789944/how-can-i-test-a-custom-delegatinghandler-in-the-asp-net-mvc-4-web-api
    public class TestHandler : DelegatingHandler {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

        public TestHandler() { _handlerFunc = (r, c) => Return200(); }

        public TestHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc) {
            _handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handlerFunc(request, cancellationToken);

        public static Task<HttpResponseMessage> Return200() => Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK));

    }
}
