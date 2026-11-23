using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Filters;

public sealed class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Знаходимо об'єкт для валідації серед аргументів запиту
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        
        if (argument is not null)
        {
            var validationResult = await validator.ValidateAsync(argument);
            
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return TypedResults.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}
