using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QuizAPI.Filters
{
    public class ValidationFilter<T> : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var model = context.Arguments.OfType<T>().FirstOrDefault();
            if (model == null)
            {
                return Results.BadRequest("Invalid request data.");
            }

            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            if (!isValid)
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
                return Results.BadRequest(new { errors });
            }

            return await next(context);
        }
    }
}