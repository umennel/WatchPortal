using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;

namespace WatchInfo.Tests
{
    [TestFixture]
    public class Tests
    {
        private ClaimsPrincipal claimsPrincipal;

        [SetUp]
        public void Setup()
        {
            var claims = new List<Claim>() 
            { 
                new Claim(ClaimTypes.NameIdentifier, "userid"),
                new Claim("tenant_id", "acme.com"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            this.claimsPrincipal = new ClaimsPrincipal(identity);
        }

        [Test]
        public void TestWatchFunctionSuccess()
        {
            var httpContext = new DefaultHttpContext();
            var queryStringValue = "abc";
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection
                (
                    new System.Collections.Generic.Dictionary<string, StringValues>()
                    {
                        { "model", queryStringValue }
                    }
                )
            };

            var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            var mock = new Mock<Security.IAccessTokenProvider>();
            mock.Setup(foo => foo.ValidateToken(request)).ReturnsAsync(this.claimsPrincipal);
            var response = new WatchPortalFunction.WatchInfo(mock.Object).Run(request, logger);
            response.Wait();

            // Check that the response is an "OK" response
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);

            // Check that the contents of the response are the expected contents
            var result = (OkObjectResult)response.Result;
            dynamic watchinfo = new { Manufacturer = "Abc", CaseType = "Solid", Bezel = "Titanium", Dial = "Roman", CaseFinish = "Silver", Jewels = 15 };
            string watchInfo = $"Watch Details: {watchinfo.Manufacturer}, {watchinfo.CaseType}, {watchinfo.Bezel}, {watchinfo.Dial}, {watchinfo.CaseFinish}, {watchinfo.Jewels}";
            Assert.AreEqual(watchInfo, result.Value);
        }

        [Test]
        public void TestWatchFunctionFailureNoQueryString()
        {
            var httpContext = new DefaultHttpContext();
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            var mock = new Mock<Security.IAccessTokenProvider>();
            mock.Setup(foo => foo.ValidateToken(request)).ReturnsAsync(this.claimsPrincipal);
            var response = new WatchPortalFunction.WatchInfo(mock.Object).Run(request, logger);
            response.Wait();

            // Check that the response is an "Bad" response
            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);

            // Check that the contents of the response are the expected contents
            var result = (BadRequestObjectResult)response.Result;
            Assert.AreEqual("Please provide a watch model in the query string", result.Value);
        }

        [Test]
        public void TestWatchFunctionFailureNoModel()
        {
            var httpContext = new DefaultHttpContext();
            var queryStringValue = "abc";
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection
                (
                    new System.Collections.Generic.Dictionary<string, StringValues>()
                    {
                        { "not-model", queryStringValue }
                    }
                )
            };

            var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            var mock = new Mock<Security.IAccessTokenProvider>();
            mock.Setup(foo => foo.ValidateToken(request)).ReturnsAsync(this.claimsPrincipal);
            var response = new WatchPortalFunction.WatchInfo(mock.Object).Run(request, logger);
            response.Wait();

            // Check that the response is an "Bad" response
            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);

            // Check that the contents of the response are the expected contents
            var result = (BadRequestObjectResult)response.Result;
            Assert.AreEqual("Please provide a watch model in the query string", result.Value);
        }
    }
}
