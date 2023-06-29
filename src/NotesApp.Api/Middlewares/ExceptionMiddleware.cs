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

            string message = exception.Message;
            HttpStatusCode statusCode;

            switch (exception)
            {
                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "Internal Server Error from the custom middleware.";
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                Message = message,
            };

            var result = JsonSerializer.Serialize(response);

            return context.Response.WriteAsync(result);
        }

    }

}
