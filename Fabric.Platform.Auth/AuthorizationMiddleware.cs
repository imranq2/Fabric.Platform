using System;
using System.Linq;
using System.Threading.Tasks;
using LibOwin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fabric.Platform.Auth
{
    public class AuthorizationMiddleware
    {
        public static AppFunc Inject(AppFunc next, string[] requiredScopes, string[] allowedPaths = null)
        {
            return env =>
            {
                var ctx = new OwinContext(env);
                if (ctx.Request.Method == "OPTIONS") return next(env);

                if (allowedPaths != null && HasPath(allowedPaths, ctx.Request.Path.Value)) return next(env);

                var principal = ctx.Request.User;
                if (principal != null)
                {
                    if (requiredScopes.Any(requiredScope => principal.HasClaim("scope", requiredScope)))
                    {
                        return next(env);
                    }
                }

                ctx.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept, Authorization" });
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "POST, GET, PUT, DELETE, PATCH" });
                ctx.Response.StatusCode = 403;

                return Task.CompletedTask;
            };
        }

        private static bool HasPath(string[] allowedPaths, string pathToCheck)
        {
            if (allowedPaths == null) return false;
            
            foreach (var path in allowedPaths)
            {
                if (path == null) continue;

                // if path matches one of the allowed path OR
                // if the path starts with an allowed path that contains an Asterisk(*) at the end
                if (path == pathToCheck ||
                    (path.LastOrDefault() == PathSymbol.AnyNumberOfCharacters &&
                    pathToCheck.StartsWith(path.TrimEnd(PathSymbol.AnyNumberOfCharacters))))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
