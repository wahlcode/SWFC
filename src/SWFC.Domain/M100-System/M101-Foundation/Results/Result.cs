namespace SWFC.Domain.M100_System.M101_Foundation.Results;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (error is null)
            throw new ArgumentNullException(nameof(error));

        if (isSuccess && error != Error.None)
            throw new ArgumentException("Successful result cannot contain an error.", nameof(error));

        if (!isSuccess && error == Error.None)
            throw new ArgumentException("Failed result must contain an error.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(false, error);
    }
}

