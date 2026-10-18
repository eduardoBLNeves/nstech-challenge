using Microsoft.AspNetCore.Diagnostics;
using Nstech.OrderService.Api;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Domain.Exceptions;

namespace Nstech.OrderService.Api.ExceptionHandling;

public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var message = Messages.UnexpectedError;

        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;
            message = appException.Message;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(message, statusCode), cancellationToken);
        return true;
    }
}
