using System.Net;
using App.Application.DTO.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace WebApp.Infrastructure;

/// <summary>
/// Replaces the default authorization failure response for API routes (/api/*) with a
/// JSON <see cref="RestApiErrorResponse"/> body so API clients receive structured errors
/// instead of an empty 401/403.  MVC routes are forwarded to the default handler which
/// redirects unauthenticated users to the login page.
/// </summary>
public class ApiAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded || !context.Request.Path.StartsWithSegments("/api"))
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        var (statusCode, message) = authorizeResult.Challenged
            ? (HttpStatusCode.Unauthorized, "Authentication is required to access this resource.")
            : (HttpStatusCode.Forbidden,    "You do not have permission to access this resource.");

        context.Response.StatusCode  = (int) statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new RestApiErrorResponse
        {
            Status = statusCode,
            Error  = message
        });
    }
}
