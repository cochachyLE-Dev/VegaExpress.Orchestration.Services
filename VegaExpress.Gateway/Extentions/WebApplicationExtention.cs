using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace VegaExpress.Gateway.Extentions
{
    internal static class WebApplicationExtention
    {
        public static RouteHandlerBuilder MapRoute(this IEndpointRouteBuilder endpoints, string method, string pattern, string location)
        {                        
            switch (method)
            {
                case "GET":
                    return endpoints.MapGet(pattern, RequestDelegate(pattern, location));                    
                case "POST":
                    return endpoints.MapPost(pattern, RequestDelegate(pattern, location));                    
                case "PUT":
                    return endpoints.MapPut(pattern, RequestDelegate(pattern, location));                    
                case "DELETE":
                    return endpoints.MapDelete(pattern, RequestDelegate(pattern, location));                    
                case "PATCH":
                    return endpoints.MapPatch(pattern, RequestDelegate(pattern, location));                    
                default:
                    throw new NotImplementedException($"Method {method} not implement!.");                    
            }
        }

        private static Delegate RequestDelegate(string pattern, string location)
        {
            var parameters = GetParameters(pattern);

            for (int i = 0; i < parameters.Length; i++)
            {
                location = location.Replace(parameters[i].match, $"{{{i}}}");                
            }

            switch (parameters.Length)
            {
                case 0:
                    {
                        Action<HttpContext> action = (httpContext) =>
                        {
                            if (!httpContext.User.Identity!.IsAuthenticated)
                            {
                                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return;
                            }

                            httpContext.Response.StatusCode = 302;
                            httpContext.Response.Redirect(location);
                        };
                        return action;
                    }                    
                case 1:
                    {
                        Action<HttpContext, string> action = (httpContext, path1) =>
                        {
                            if (!httpContext.User.Identity!.IsAuthenticated)
                            {
                                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return;
                            }

                            var newPath = Regex.IsMatch(location, @"\{(\d+)\}")? string.Format(location, path1) : location;

                            httpContext.Response.StatusCode = 302;
                            httpContext.Response.Redirect(newPath);
                        };
                        return action;
                    }
                case 2:
                    {
                        Action<HttpContext, string, string> action = (httpContext, path1, path2) =>
                        {
                            if (!httpContext.User.Identity!.IsAuthenticated)
                            {
                                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return;
                            }

                            var newPath = Regex.IsMatch(location, @"\{(\d+)\}") ? string.Format(location, path1, path2) : location;

                            httpContext.Response.StatusCode = 302;
                            httpContext.Response.Redirect(newPath);
                        };
                        return action;
                    }
                case 3:
                    {
                        Action<HttpContext, string, string, string> action = (httpContext, path1, path2, path3) =>
                        {
                            if (!httpContext.User.Identity!.IsAuthenticated)
                            {
                                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return;
                            }
                            
                            var newPath = Regex.IsMatch(location, @"\{(\d+)\}") ? string.Format(location, path1, path2, path3) : location;

                            httpContext.Response.StatusCode = 302;
                            httpContext.Response.Redirect(newPath);
                        };
                        return action;
                    }
                case 4:
                    {
                        Action<HttpContext, string, string, string, string> action = (httpContext, path1, path2, path3, path4) =>
                        {
                            if (!httpContext.User.Identity!.IsAuthenticated)
                            {
                                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return;
                            }
                            
                            var newPath = Regex.IsMatch(location, @"\{(\d+)\}") ? string.Format(location, path1, path2, path3, path4) : location;

                            httpContext.Response.StatusCode = 302;
                            httpContext.Response.Redirect(newPath);
                        };
                        return action;
                    }
                case 5:
                    {
                        Action<HttpContext, string, string, string, string, string> action = (httpContext, path1, path2, path3, path4, path5) =>
                        {
                            if (!httpContext.User.Identity!.IsAuthenticated)
                            {
                                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return;
                            }
                            
                            var newPath = Regex.IsMatch(location, @"\{(\d+)\}") ? string.Format(location, path1, path2, path3, path4, path5) : location;

                            httpContext.Response.StatusCode = 302;
                            httpContext.Response.Redirect(newPath);
                        };
                        return action;
                    }                
                default:
                    throw new ArgumentOutOfRangeException($"pattern: {pattern}");                    
            }
        }

        private static LambdaExpression Redirect(string location, params string[] paths)
        {
            var contextParam = Expression.Parameter(typeof(HttpContext), "context");
            var responseProp = Expression.Property(contextParam, typeof(HttpContext).GetProperty("Response")!);
            var redirectMethod = typeof(HttpResponse).GetMethod("Redirect", new Type[] { typeof(string) });
            var formatMethod = typeof(string).GetMethod("Format", new[] { typeof(string) }.Concat(Enumerable.Repeat(typeof(object), paths.Length)).ToArray())!;
            var locationExpr = Expression.Constant(location);
            var pathParams = paths.Select((path, i) => Expression.Parameter(typeof(string), $"path{i + 1}")).ToArray();
            var redirectCall = Expression.Call(responseProp, redirectMethod!, Expression.Call(formatMethod, new Expression[] { locationExpr }.Concat(pathParams).ToArray()));
            var lambdaParams = new[] { contextParam }.Concat(pathParams).ToArray();
            var delegateType = Expression.GetFuncType(lambdaParams.Select(p => p.Type).Concat(new[] { typeof(Task) }).ToArray());
            return Expression.Lambda(delegateType, Expression.Block(redirectCall, Expression.Constant(Task.CompletedTask)), lambdaParams);
        }
        private static LambdaExpression Redirect3(string location, params string[] paths)
        {
            var contextParam = Expression.Parameter(typeof(HttpContext), "context");
            var responseProp = Expression.Property(contextParam, typeof(HttpContext).GetProperty("Response")!);
            var redirectMethod = typeof(HttpResponse).GetMethod("Redirect", new Type[] { typeof(string) });
            var formatMethod = typeof(string).GetMethod("Format", new[] { typeof(string) }.Concat(Enumerable.Repeat(typeof(object), paths.Length)).ToArray())!;
            var locationExpr = Expression.Constant(location);
            var pathParams = paths.Select((path, i) => Expression.Parameter(typeof(string), $"path{i + 1}")).ToArray();
            var redirectCall = Expression.Call(responseProp, redirectMethod!, Expression.Call(formatMethod, new Expression[] { locationExpr }.Concat(pathParams).ToArray()));
            var lambdaParams = new[] { contextParam }.Concat(pathParams).ToArray();
            var delegateType = Expression.GetFuncType(lambdaParams.Select(p => p.Type).Concat(new[] { typeof(Task) }).ToArray());
            return Expression.Lambda<Func<HttpContext, Task>>(Expression.Block(redirectCall, Expression.Constant(Task.CompletedTask)), contextParam);
        }
    
        private static LambdaExpression Redirect2(string location, params string[] paths)
        {
            //var contextParam = Expression.Parameter(typeof(HttpContext), "context");
            //var responseProp = Expression.Property(contextParam, typeof(HttpContext).GetProperty("Response")!);            
            //var redirectMethod = typeof(HttpResponse).GetMethod("Redirect", new Type[] { typeof(string) });
            //var formatMethod = typeof(string).GetMethod("Format", new[] { typeof(string) }.Concat(Enumerable.Repeat(typeof(object), paths.Length)).ToArray())!;
            //var locationExpr = Expression.Constant(location);
            //var pathParams = paths.Select((path, i) => Expression.Parameter(typeof(string), $"path{i + 1}")).ToArray();                               
            //var redirectCall = Expression.Call(responseProp, redirectMethod!, Expression.Call(formatMethod, new Expression[] { locationExpr }.Concat(pathParams).ToArray()));
            //var lambdaParams = new[] { contextParam }.Concat(pathParams).ToArray();
            //var delegateType = Expression.GetFuncType(lambdaParams.Select(p => p.Type).Concat(new[] { typeof(Task) }).ToArray());
            //return Expression.Lambda(delegateType, redirectCall, lambdaParams);

            //var contextParam = Expression.Parameter(typeof(HttpContext), "context");
            //var responseProp = Expression.Property(contextParam, typeof(HttpContext).GetProperty("Response")!);
            //var redirectMethod = typeof(HttpResponse).GetMethod("Redirect", new Type[] { typeof(string) });
            //var formatMethod = typeof(string).GetMethod("Format", new[] { typeof(string) }.Concat(Enumerable.Repeat(typeof(object), paths.Length)).ToArray())!;
            //var locationExpr = Expression.Constant(location);
            //var pathParams = paths.Select((path, i) => Expression.Parameter(typeof(string), $"path{i + 1}")).ToArray();
            //var redirectCall = Expression.Call(responseProp, redirectMethod!, Expression.Call(formatMethod, new Expression[] { locationExpr }.Concat(pathParams).ToArray()));
            //var lambdaParams = new[] { contextParam }.Concat(pathParams).ToArray();
            //var delegateType = Expression.GetActionType(lambdaParams.Select(p => p.Type).ToArray());
            //return Expression.Lambda(delegateType, redirectCall, lambdaParams);

            //var contextParam = Expression.Parameter(typeof(HttpContext), "context");
            //var responseProp = Expression.Property(contextParam, typeof(HttpContext).GetProperty("Response")!);
            //var redirectMethod = typeof(HttpResponse).GetMethod("Redirect", new Type[] { typeof(string) });
            //var formatMethod = typeof(string).GetMethod("Format", new[] { typeof(string) }.Concat(Enumerable.Repeat(typeof(object), paths.Length)).ToArray())!;
            //var locationExpr = Expression.Constant(location);
            //var pathParams = paths.Select((path, i) => Expression.Parameter(typeof(string), $"path{i + 1}")).ToArray();
            //var redirectCall = Expression.Call(responseProp, redirectMethod!, Expression.Call(formatMethod, new Expression[] { locationExpr }.Concat(pathParams).ToArray()));
            //var lambdaParams = new[] { contextParam }.Concat(pathParams).ToArray();
            //var delegateType = Expression.GetFuncType(lambdaParams.Select(p => p.Type).Concat(new[] { typeof(Task) }).ToArray());
            //return Expression.Lambda(delegateType, Expression.Block(redirectCall, Expression.Constant(Task.CompletedTask)), lambdaParams);

            var contextParam = Expression.Parameter(typeof(HttpContext), "context");
            var responseProp = Expression.Property(contextParam, typeof(HttpContext).GetProperty("Response")!);
            var redirectMethod = typeof(HttpResponse).GetMethod("Redirect", new Type[] { typeof(string) });
            var formatMethod = typeof(string).GetMethod("Format", new[] { typeof(string) }.Concat(Enumerable.Repeat(typeof(object), paths.Length)).ToArray())!;
            var locationExpr = Expression.Constant(location);
            var pathParams = paths.Select((path, i) => Expression.Parameter(typeof(string), $"path{i + 1}")).ToArray();
            var redirectCall = Expression.Call(responseProp, redirectMethod!, Expression.Call(formatMethod, new Expression[] { locationExpr }.Concat(pathParams).ToArray()));
            var lambdaParams = new[] { contextParam }.Concat(pathParams).ToArray();
            var delegateType = Expression.GetFuncType(lambdaParams.Select(p => p.Type).Concat(new[] { typeof(Task) }).ToArray());
            return Expression.Lambda(delegateType, Expression.Block(redirectCall, Expression.Constant(Task.CompletedTask)), lambdaParams);
        }

        private static Expression<Func<HttpContext, Task>> Redirect(string location) => Expression.Lambda<Func<HttpContext, Task>>(
                            Expression.Call(
                                Expression.Property(
                                    Expression.Parameter(typeof(HttpContext), "context"),
                                    typeof(HttpContext).GetProperty("Response")!
                                ),
                                typeof(HttpResponse).GetMethod("Redirect")!,
                                Expression.Constant(location)
                            ),
                            Expression.Parameter(typeof(HttpContext), "context")
                        );
        
        private static Delegate RedirectWithAuthorization(string location, string[] paths)
        {
            // Parameters
            var contextParam = Expression.Parameter(typeof(HttpContext), "context");
            var pathParam = Expression.Parameter(typeof(string), "path");

            // Properties
            var userProp = Expression.Property(contextParam, typeof(HttpContext).GetProperty("User")!);
            var identityProp = Expression.Property(userProp, typeof(System.Security.Claims.ClaimsPrincipal).GetProperty("Identity")!);
            var isAuthenticatedProp = Expression.Property(identityProp, typeof(System.Security.Principal.IIdentity).GetProperty("IsAuthenticated")!);
            var responseProp = Expression.Property(contextParam, typeof(HttpContext).GetProperty("Response")!);
            var statusCodeProp = Expression.Property(responseProp, typeof(HttpResponse).GetProperty("StatusCode")!);

            // Methods            
            var redirectMethod = typeof(HttpResponse).GetMethod("Redirect", new Type[] { typeof(string) });
            var formatMethod = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) })!;

            // Expressions
            var notAuthenticatedExpr = Expression.Not(isAuthenticatedProp);
            var statusCodeAssignExpr = Expression.Constant(StatusCodes.Status401Unauthorized, typeof(int));            

            // Redirect call expression
            var redirectCallExpr = Expression.Call(responseProp, redirectMethod!, Expression.Call(formatMethod, Expression.Constant("https://127.0.0.1:8091/home/{0}"), pathParam));            

            var condition = Expression.IfThenElse(notAuthenticatedExpr, Expression.Assign(statusCodeProp, statusCodeAssignExpr), redirectCallExpr);

            // Final Lambda Expression            
            var lambdaExpr = Expression.Lambda<Action<HttpContext, string>>(condition, contextParam, pathParam);

            // Compile the lambda expression
            var @delegate = lambdaExpr.Compile();
            return @delegate;
        }
        private static Expression<Func<HttpContext, string, Task>> RedirectWithAuthorization2(string location, string[] paths) => Expression.Lambda<Func<HttpContext, string, Task>>(
                    Expression.Block(
                        Expression.IfThen(
                            Expression.Not(
                                Expression.Property(
                                    Expression.Property(
                                        Expression.Property(
                                            Expression.Parameter(typeof(HttpContext), "context"),
                                            typeof(HttpContext).GetProperty("User")!
                                        ),
                                        typeof(ClaimsPrincipal).GetProperty("Identity")!
                                    ),
                                    typeof(IIdentity).GetProperty("IsAuthenticated")!
                                )
                            ),
                            Expression.Block(
                                Expression.Assign(
                                    Expression.Property(
                                        Expression.Property(
                                            Expression.Parameter(typeof(HttpContext), "context"),
                                            typeof(HttpContext).GetProperty("Response")!
                                        ),
                                        typeof(HttpResponse).GetProperty("StatusCode")!
                                    ),
                                    Expression.Constant(StatusCodes.Status401Unauthorized)
                                ),
                                Expression.Return(Expression.Label())
                            )
                        ),
                        Expression.Call(
                            Expression.Property(
                                Expression.Parameter(typeof(HttpContext), "context"),
                                typeof(HttpContext).GetProperty("Response")!
                            ),
                            typeof(HttpResponse).GetMethod("Redirect")!,
                            Expression.Call(
                                typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) })!,
                                Expression.Constant(location),
                                Expression.Parameter(typeof(string), "path")
                            )
                        )
                    ),
                    Expression.Parameter(typeof(HttpContext), "context"),
                    Expression.Parameter(typeof(string), "path")
                    );        

        static (string match, string value)[] GetParameters(string pattern)
        {
            Regex regex = new Regex(@"\{(\*?)(\w+)\}");
            MatchCollection matches = regex.Matches(pattern);

            List<(string, string)> words = new List<(string, string)>();
            foreach (Match match in matches)
            {                
                words.Add((match.Value, match.Groups[2].Value));
            }

            return words.ToArray();
        }
    }
}
