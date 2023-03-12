using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Knapcode.FactorioTools.WebApp;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ProblemDetailsFactory _factory;

    public ExceptionFilter(ProblemDetailsFactory factory)
    {
        _factory = factory;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is FactorioToolsException ex)
        {
            var problemDetails = _factory.CreateProblemDetails(
                context.HttpContext,
                statusCode: ex.BadInput ? 400 : 500,
                title: ex.BadInput ? "Bad input was provided." : "A FactorioTools exception occurred.");

            var errors = new List<string>();
            Exception? exception = ex;
            while (exception != null)
            {
                errors.Add(exception.Message);
                exception = exception.InnerException;
            }

            problemDetails.Extensions["errors"] = new Dictionary<string, List<string>>
            {
                { nameof(FactorioToolsException), errors }
            };

            context.Result = new ObjectResult(problemDetails);
            context.ExceptionHandled = true;
        }
    }
}
