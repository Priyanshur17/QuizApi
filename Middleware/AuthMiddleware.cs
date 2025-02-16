using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using QuizAPI.Data;
using QuizAPI.Models;

namespace QuizAPI.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext dbContext)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Not authorized, no token");
                return;
            }

            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
                var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

                if (userId == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Not authorized, token invalid");
                    return;
                }

                var user = dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);

                if (user == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Not authorized, user not found");
                    return;
                }

                context.Items["User"] = user;
            }
            catch (Exception)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Not authorized, token failed");
                return;
            }

            await _next(context);
        }
    }
}
