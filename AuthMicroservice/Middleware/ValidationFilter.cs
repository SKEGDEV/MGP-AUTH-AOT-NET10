using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AuthMicroservice.Core.Exceptions;
using AuthMicroservice.Core.DTOs.Responses;
using AuthMicroservice.Core.Dictionaries;

namespace AuthMicroservice.Middleware;

public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument == null) continue;

            var validationContext = new ValidationContext(argument);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(argument, validationContext, validationResults, true))
            {
                var errors = new List<StandardError>();
                var errorCodes = new List<string>();

                foreach (var validationResult in validationResults)
                {
                    // The ErrorMessage from DataAnnotations contains the Code, e.g., "VAL001"
                    var code = validationResult.ErrorMessage ?? "VAL000N";
                    var message = MessageDictionary.GetMessage(code);
                    
                    errorCodes.Add(code);
                    
                    errors.Add(new StandardError
                    {
                        ErrorCode = code,
                        ErrorComponent = "Validation",
                        ErrorMessage = message
                    });
                }

                // Format string like "VAL001 | VAL002 | VAL000N"
                var formattedCodes = string.Join(" | ", errorCodes);

                // Throw custom exception which will be caught by ExceptionHandlingMiddleware
                throw new CustomAuthException(400, "VAL000N", formattedCodes, errors);
            }
        }

        return await next(context);
    }
}
