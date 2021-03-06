﻿using BuildFunc = System.Action<System.Func<
                    System.Func<
                        System.Collections.Generic.IDictionary<string, object>,
                        System.Threading.Tasks.Task>,
                    System.Func<
                        System.Collections.Generic.IDictionary<string, object>,
                        System.Threading.Tasks.Task>
                   >>;

namespace Fabric.Platform.Auth
{
    public static class BuildFuncExtensions
    {
        public static BuildFunc UseAuthPlatform(this BuildFunc buildFunc, string[] requiredScopes, string[] allowedPaths = null)
        {
            buildFunc(next => AuthorizationMiddleware.Inject(next, requiredScopes, allowedPaths));
            return buildFunc;
        }
    }
}
