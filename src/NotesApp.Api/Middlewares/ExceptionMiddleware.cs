using NotesApp.Application.Response;
using System.Net;
using System.Text.Json;

namespace NotesApp.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                var path = httpContext.Request.Path;
                var method = httpContext.Request.Method;
                _logger.LogError($"Something went wrong in {method} {path}: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ServiceResponse<string>
            {
                Data = "Internal Server Error from the custom middleware.",
                Message = "Error : " + exception.Message,
                Success = false,
            };

            var result = JsonSerializer.Serialize(response);

            return context.Response.WriteAsync(result);
        }
    }

}
