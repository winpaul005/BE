using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace Middleware.Trace;
public class RequestTraceMiddleware
{
    private readonly RequestDelegate _next;
 
    public RequestTraceMiddleware(RequestDelegate next)
    {
        this._next = next;
    }
 
    public async Task InvokeAsync(HttpContext context)
    {
        Guid asdf = Guid.NewGuid();
        context.Response.Headers.Add("X-Trace-Id",asdf.ToString());
        context.Items["TraceId"] = context.Response.Headers["X-Trace-Id"];
        await _next.Invoke(context);
    }
}