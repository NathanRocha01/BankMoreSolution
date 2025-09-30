using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Shared.Errors;
using Shared.Exceptions;
using System.Net;
using System.Text.Json;
using static Shared.Errors.ErrorCodes;

namespace Shared.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                var statusCode = MapErrorCodeToStatus(ex.ErrorCode);
                await HandleExceptionAsync(context, statusCode, ex.ErrorCode, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                var domainEx = new DomainException(ex.Message, ErrorCodes.UserUnauthorized);
                var statusCode = MapErrorCodeToStatus(domainEx.ErrorCode);

                await HandleExceptionAsync(context, statusCode, domainEx.ErrorCode, domainEx.Message);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "INTERNAL_ERROR", ex.Message);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string errorType, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                type = errorType,
                message
            });

            return context.Response.WriteAsync(result);
        }
        private static HttpStatusCode MapErrorCodeToStatus(string errorCode)
        {
            return errorCode switch
            {
                ErrorCodes.UserUnauthorized => HttpStatusCode.Unauthorized, 
                ErrorCodes.InvalidToken => HttpStatusCode.Forbidden,        
                _ => HttpStatusCode.BadRequest                              
            };
        }
    }
    public static class ExceptionMiddlewareExtensions 
    { 
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder) 
        { 
            return builder.UseMiddleware<ExceptionMiddleware>(); 
        } 
    }
}
