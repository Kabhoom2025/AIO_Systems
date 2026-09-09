using Chatbot.Application.Common.Exceptions;
using FluentValidation;

namespace Chatbot.Application.Common.Validation;

public static class ValidationExtensions
{
    public static async Task ValidateAndThrowAppExceptionAsync<T>(this IValidator<T> validator, T instance, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ValidationAppException(errors);
        }
    }
}
