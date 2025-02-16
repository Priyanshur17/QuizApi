using Microsoft.AspNetCore.Mvc;
using QuizAPI.Controllers;
using QuizAPI.Validators;
using Microsoft.Extensions.DependencyInjection;
using QuizAPI.Filters;
namespace QuizAPI.Routes
{
    public static class UserRoutes
    {
        public static void MapUserRoutes(this IEndpointRouteBuilder endpoints)
        {
            // Resolve UserController from the DI container
            var userController = endpoints.ServiceProvider.GetRequiredService<UserController>();

            endpoints.MapPost("/login", userController.LoginUser)
                .AddEndpointFilter<ValidationFilter<LoginValidator>>();

            endpoints.MapPost("/send-reset-password-email", userController.SendResetPasswordEmail)
                .AddEndpointFilter<ValidationFilter<SendResetPasswordEmailValidator>>();

            endpoints.MapPost("/reset-password/{id}/{token}", userController.ResetPassword)
                .AddEndpointFilter<ValidationFilter<ResetPasswordValidator>>();

            endpoints.MapPost("/register", userController.RegisterUser)
                .AddEndpointFilter<ValidationFilter<RegisterValidator>>();

            endpoints.MapGet("/verify-email/{id}/{token}", userController.VerifyEmail);
        }
    }
}