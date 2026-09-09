namespace FlowSphere.Application.Common;

/// <summary>Marker so MediatR pipeline behaviors (e.g. ValidationBehavior) can constrain
/// TResponse to "any Result-shaped response" without knowing the generic argument.</summary>
public interface IResult
{
    bool IsSuccess { get; }
    Error? Error { get; }
}

public class Result : IResult
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : IResult
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(Error error) => new(false, default, error);
}
