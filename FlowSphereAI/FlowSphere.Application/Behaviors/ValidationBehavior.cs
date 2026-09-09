using FluentValidation;
using MediatR;
using FlowSphere.Application.Common;

namespace FlowSphere.Application.Behaviors;

/// <summary>
/// Runs all registered FluentValidation validators for TRequest. On failure it returns a
/// Result/Result&lt;T&gt;.Failure(validationError) WITHOUT throwing, short-circuiting the
/// handler. Requires TResponse to be Result or Result&lt;T&gt; (enforced at MediatR
/// registration time by convention - every Command/Query in this codebase returns one of these).
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var validationErrors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        var error = Error.Validation(validationErrors);

        return BuildFailureResponse(error);
    }

    private static TResponse BuildFailureResponse(Error error)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(nameof(Result<object>.Failure));
            return (TResponse)failureMethod!.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException(
            $"{responseType.Name} is not a Result/Result<T> - ValidationBehavior only supports Result-shaped responses.");
    }
}
