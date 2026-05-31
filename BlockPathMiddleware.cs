using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Middleware.Blockpath;
public class BlockPathMiddleware
{
    private readonly RequestDelegate _next;
 
    public BlockPathMiddleware(RequestDelegate next)
    {
        this._next = next;
    }
 
    public async Task InvokeAsync(HttpContext context)
    {
        if(context.Request.Path.ToString().StartsWith("/blocked"))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("403");
        }
        await _next.Invoke(context);
    }
}