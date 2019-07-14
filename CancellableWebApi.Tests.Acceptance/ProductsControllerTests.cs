using CancellableWebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class ProductsControllerTests
    {
        private const string _baseUrl = "api/products";

        private HttpClient _httpClient;
        private TestServer _testServer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var builder = new WebHostBuilder()
           .UseContentRoot(GetContentRootPath())
           .UseEnvironment("Development")
           .UseStartup<Startup>();

            _testServer = new TestServer(builder);
        }

        [SetUp]
        public void Setup()
        {
            _httpClient = _testServer.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _testServer.Dispose();
        }

        [Test]
        [TestCase(100, 200)]
        public async Task Should_Return_Products_When_NOT_TimedOut(int waitTime, int timeoutTime)
        {
            //given
            var cancellationTokenSource = new CancellationTokenSource(timeoutTime);
            var url = $"{_baseUrl}?waitTime={waitTime}";

            //when
            var response = await _httpClient.GetAsync(url, cancellationTokenSource.Token);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<IEnumerable<string>>(responseString);

            //then
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(products);
            Assert.AreEqual(1, products.Count());
        }

        [Test]
        [TestCase(200, 100)]
        public void Should_Abort_Request_When_TimedOut(int waitTime, int timeoutTime)
        {
            //given
            var cancellationTokenSource = new CancellationTokenSource(timeoutTime);
            var url = $"{_baseUrl}?waitTime={waitTime}";

            //when
            Assert.ThrowsAsync<TaskCanceledException>(()=>_httpClient.GetAsync(url, cancellationTokenSource.Token));
        }

        [Test]
        [TestCase(100)]
        public void Should_Abort_Request_When_Client_Cancels_Request(int waitTime)
        {
            //given
            var cancellationTokenSource = new CancellationTokenSource();
            var url = $"{_baseUrl}?waitTime={waitTime}";

            //when
            Assert.ThrowsAsync<TaskCanceledException>(() =>
            {
                var task = _httpClient.GetAsync(url, cancellationTokenSource.Token);
                _httpClient.CancelPendingRequests();

                return task;
            });
        }

        private string GetContentRootPath()
        {
            var testProjectPath = PlatformServices.Default.Application.ApplicationBasePath;
            var relativePathToHostProject = @"..\..\..\..\CancellableWebApi";
            return Path.Combine(testProjectPath, relativePathToHostProject);
        }
    }
}