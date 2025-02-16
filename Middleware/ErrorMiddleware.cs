using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace QuizAPI.Middleware
{
    public class ErrorMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = context.Response.StatusCode == 200 ? 500 : context.Response.StatusCode;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(new
            {
                message = exception.Message,
                stack = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" ? null : exception.StackTrace
            }.ToString());
        }
    }
}
