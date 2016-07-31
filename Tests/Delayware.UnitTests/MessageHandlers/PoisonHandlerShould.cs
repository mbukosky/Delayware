using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Delayware.MessageHandlers;
using Delayware.MessageHandlers.Strategies;
using JWT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Delayware.UnitTests.MessageHandlers {
    [TestClass]
    public class PoisonHandlerShould {

        private PoisonHandler _sut;
        private HttpRequestMessage _message;

        private const string JWT_SECRET = "star_lord";
        private const string POISON_HEADER = "X-POISON";

        [TestInitialize]
        public void Setup() {
            _message = new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1");
            _sut = new PoisonHandler(new DefaultPoisonStrategy(JWT_SECRET)) {
                InnerHandler = new TestHandler((r, c) => TestHandler.Return200())
            };
        }

        [TestMethod]
        public async Task Delay_One_Request() {
            // Arrange
            var payload = new {
                from = 500,
                to = 1000,
                action = "single",
                type = "delay"
            };
            var jwt = JsonWebToken.Encode(payload, JWT_SECRET, JwtHashAlgorithm.HS256);
            var client = new HttpClient(_sut);

            // Act
            _message.Headers.Add(POISON_HEADER, jwt);
            var result = await client.SendAsync(_message);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Error_One_Request() {
            // Arrange
            var payload = new {
                code = 500,
                from = 500,
                to = 1000,
                action = "single",
                type = "status"
            };
            var jwt = JsonWebToken.Encode(payload, JWT_SECRET, JwtHashAlgorithm.HS256);
            var client = new HttpClient(_sut);

            // Act
            _message.Headers.Add(POISON_HEADER, jwt);
            var result = await client.SendAsync(_message);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.InternalServerError);
        }

        [TestMethod]
        public async Task BadRequest_One_Request() {
            // Arrange
            var payload = new {
                code = 400,
                action = "single",
                type = "status"
            };
            var jwt = JsonWebToken.Encode(payload, JWT_SECRET, JwtHashAlgorithm.HS256);
            var client = new HttpClient(_sut);

            // Act
            _message.Headers.Add(POISON_HEADER, jwt);
            var result = await client.SendAsync(_message);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Delay_Many_Requests_For_A_Duration() {
            // Arrange
            var payload = new {
                from = 500,
                to = 1000,
                duration = 30, //seconds
                action = "timed",
                type = "delay"
            };
            var jwt = JsonWebToken.Encode(payload, JWT_SECRET, JwtHashAlgorithm.HS256);
            var client = new HttpClient(_sut);

            // Act 
            _message.Headers.Add(POISON_HEADER, jwt);
            var result = await client.SendAsync(_message);

            var manyResults = Enumerable.Range(0, 5).Select(x => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1")));
            var results = await Task.WhenAll(manyResults);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            foreach (var res in results) {
                Assert.AreEqual(res.StatusCode, HttpStatusCode.OK);
            }
        }

        [TestMethod]
        public async Task InteranlServerError_Many_Requests_For_A_Duration() {
            // Arrange
            var payload = new {
                code = 500,
                duration = 30, //seconds
                action = "timed",
                type = "status"
            };
            var jwt = JsonWebToken.Encode(payload, JWT_SECRET, JwtHashAlgorithm.HS256);
            var message = new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1");
            message.Headers.Add(POISON_HEADER, jwt);

            var client = new HttpClient(_sut);

            var result = await client.SendAsync(message);

            var manyResults = Enumerable.Range(0, 5).Select(x => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1")));
            var results = await Task.WhenAll(manyResults);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.InternalServerError);
            foreach (var res in results) {
                Assert.AreEqual(res.StatusCode, HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        public async Task Override_An_Existing_Poison() {
            // Arrange
            var client = new HttpClient(_sut);

            var serverErrorPayload = new {
                code = 500,
                duration = 30, //seconds
                action = "timed",
                type = "status"
            };

            var badRequestPayload = new {
                code = 400,
                duration = 30, //seconds
                action = "timed",
                type = "status"
            };

            // 500 poison
            var serverErrorMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1");
            serverErrorMessage.Headers.Add(POISON_HEADER, JsonWebToken.Encode(serverErrorPayload, JWT_SECRET, JwtHashAlgorithm.HS256));

            var firstResult = await client.SendAsync(serverErrorMessage);
            Assert.AreEqual(firstResult.StatusCode, HttpStatusCode.InternalServerError);

            // 400 poison
            var badRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1");
            badRequestMessage.Headers.Add(POISON_HEADER, JsonWebToken.Encode(badRequestPayload, JWT_SECRET, JwtHashAlgorithm.HS256));

            var secondResult = await client.SendAsync(badRequestMessage);
            Assert.AreEqual(secondResult.StatusCode, HttpStatusCode.BadRequest);

            // Act
            var manyResults = Enumerable.Range(0, 5).Select(x => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1")));
            var results = await Task.WhenAll(manyResults);

            // Assert
            foreach (var res in results) {
                Assert.AreEqual(res.StatusCode, HttpStatusCode.BadRequest);
            }
        }

        [TestMethod]
        public async Task Clear_Poison() {
            // Arrange
            var client = new HttpClient(_sut);

            var serverErrorPayload = new {
                code = 500,
                duration = 30, //seconds
                action = "timed",
                type = "status"
            };

            var clearPayload = new {
                type = "clear"
            };

            // 500 poison
            var serverErrorMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1");
            serverErrorMessage.Headers.Add(POISON_HEADER, JsonWebToken.Encode(serverErrorPayload, JWT_SECRET, JwtHashAlgorithm.HS256));

            var firstResult = await client.SendAsync(serverErrorMessage);
            Assert.AreEqual(firstResult.StatusCode, HttpStatusCode.InternalServerError);

            // clear poison
            var clearMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1");
            clearMessage.Headers.Add(POISON_HEADER, JsonWebToken.Encode(clearPayload, JWT_SECRET, JwtHashAlgorithm.HS256));

            var secondResult = await client.SendAsync(clearMessage);
            Assert.AreEqual(secondResult.StatusCode, HttpStatusCode.OK);

            // Act
            var manyResults = Enumerable.Range(0, 5).Select(x => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test.com/v1")));
            var results = await Task.WhenAll(manyResults);

            // Assert
            foreach (var res in results) {
                Assert.AreEqual(res.StatusCode, HttpStatusCode.OK);
            }
        }
    }
}
