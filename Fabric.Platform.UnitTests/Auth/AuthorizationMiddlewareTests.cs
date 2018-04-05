using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using System.Threading.Tasks;
using Fabric.Platform.Auth;
using Fabric.Platform.UnitTests.Mocks;
using LibOwin;
using Xunit;

using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fabric.Platform.UnitTests.Auth
{
    public class AuthorizationMiddlewareTests
    {
        private readonly AppFunc _noOp = env => Task.CompletedTask;

        [Theory, MemberData(nameof(RequestUser))]
        public void AuthorizationMiddleware_Inject_ReturnsForbiddenResponse(ClaimsPrincipal claimsPrincipal, string method, HttpStatusCode statusCode)
        {
            var ctx = new OwinContext
            {
                Request =
                {
                    Scheme = LibOwin.Infrastructure.Constants.Https,
                    Path = new PathString("/authtest"),
                    Method = method,
                    User = claimsPrincipal
                }
            };


            var pipeline = AuthorizationMiddleware.Inject(_noOp, new[] { "api1.read", "api1.write" });
            pipeline(ctx.Environment);
            Assert.Equal((int)statusCode, ctx.Response.StatusCode);
        }

        [Theory, MemberData(nameof(AllowedPathsRequests))]
        public void AuthorizationMiddleware_Inject_AllowsAllowedPaths(ClaimsPrincipal claimsPrincipal, string method, string pathValue, string[] allowedPaths, HttpStatusCode statusCode)
        {
            var ctx = new OwinContext
            {
                Request =
                {
                    Scheme = LibOwin.Infrastructure.Constants.Https,
                    Path = new PathString(pathValue),
                    Method = method,
                    User = claimsPrincipal
                }
            };


            var pipeline = AuthorizationMiddleware.Inject(_noOp, new[] { "api1.read", "api1.write" }, allowedPaths);
            pipeline(ctx.Environment);
            Assert.Equal((int)statusCode, ctx.Response.StatusCode);
        }

        public static IEnumerable<object[]> AllowedPathsRequests => new[]
        {
            new object[]{ new TestPrincipal(), "GET", "/authtest", new []{"/authtest"}, HttpStatusCode.OK },
            new object[]{ new TestPrincipal(), "GET", "/authtest", null, HttpStatusCode.Forbidden },
            new object[]{ new TestPrincipal(), "GET", "/authtest", new []{"/foo", "/authtest"}, HttpStatusCode.OK },
            new object[]{ new TestPrincipal(), "GET", "/authtest", new []{"/foo"}, HttpStatusCode.Forbidden },            
        };

        public static IEnumerable<object[]> RequestUser => new[]
        {
            new object[] { new TestPrincipal(new Claim("scope", "api1.read")), "GET", HttpStatusCode.OK },
            new object[] { new TestPrincipal(), "GET", HttpStatusCode.Forbidden },
            new object[] { new TestPrincipal(), "OPTIONS", HttpStatusCode.OK }
        };
    }
}
