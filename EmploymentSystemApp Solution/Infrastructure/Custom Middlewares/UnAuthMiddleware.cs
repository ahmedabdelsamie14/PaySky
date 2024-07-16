namespace Infrastructure.Custom_Middlewares
{
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public class UnAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public UnAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\":\"You are not authorized to access this resource.\"}");
            }
        }
    }
}
