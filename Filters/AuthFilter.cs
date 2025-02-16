using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using QuizAPI.Data;

namespace QuizAPI.Filters
{
    public class AuthFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthFilter(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            Console.WriteLine($"Token: {token}");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ObjectResult(new { success = false, message = "Unauthorized" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
                var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                Console.WriteLine($"User ID from token: {userId}");

                if (userId == null)
                {
                    context.Result = new ObjectResult(new { success = false, message = "Unauthorized" })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }

                var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
                Console.WriteLine($"User found: {user != null}");

                if (user == null)
                {
                    context.Result = new ObjectResult(new { success = false, message = "Unauthorized" })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }

                context.HttpContext.Items["UserId"] = userId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AuthFilter: {ex.Message}");
                context.Result = new ObjectResult(new { success = false, message = "Invalid token" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            await next();
        }
    }
}
