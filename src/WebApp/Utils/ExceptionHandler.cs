using System;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;

namespace WebApp.Utils
{
    public class ExceptionHandler : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionHandler> _logger;
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        public ExceptionHandler(ILogger<ExceptionHandler> logger, ProblemDetailsFactory problemDetailsFactory)
        {
            _logger = logger;
            _problemDetailsFactory = problemDetailsFactory;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Exception occured");
            var problemDetails = HandleException(context);

            if (problemDetails == default)
            {
                _logger.LogError(context.Exception, "Exception thrown but no one can catch it");
                return;
            }

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            context.ExceptionHandled = true;
        }

        private ProblemDetails HandleException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ArgumentException argumentException:
                {
                    if (!string.IsNullOrEmpty(argumentException.ParamName))
                    {
                        context.ModelState.AddModelError(argumentException.ParamName, argumentException.Message);
                    }
                    var problemDetails = _problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext,
                        context.ModelState);
                    problemDetails.Detail = argumentException.Message;


                    return problemDetails;
                }

                case InvalidOperationException invalidOperationException:
                {
                    var problemDetails = _problemDetailsFactory.CreateProblemDetails(context.HttpContext);
                    problemDetails.Detail = invalidOperationException.Message;
                    return problemDetails;
                }

                case ValidationException validationException:
                {
                    foreach (var item in validationException.Errors)
                    {
                        context.ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                    }

                    var problemDetails = _problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
                    problemDetails.Detail = "Your request cannot pass our validations. Sorry";
                    return problemDetails;
                }

                case ApiErrorException apiErrorException:
                {
                    var detail = apiErrorException.Body.Error.Message;
                    var problemDetails = _problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
                    problemDetails.Detail = detail;
                    return problemDetails;
                }

                default:
                    return default;
            }
        }
    }
}
