using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using BE.Models;

namespace Middleware.Setcounter;
public class SetCounterMiddleware
{
    private readonly RequestDelegate _next;
    public SetCounterMiddleware(RequestDelegate next)
    {
        _next = next;
    } 
    public async Task InvokeAsync(HttpContext context, UsersDbContext _dbContext)
    {
        _dbContext.Props.ToList()[0].totaltasks = _dbContext.Tasks.Count();
        _dbContext.SaveChanges();
        await _next(context); 
        
    }
}