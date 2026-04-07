namespace MusicMigrater.BLL.DTO.Result;

public interface IAppResult
{
    bool IsSuccess { get; }

    string Error { get; }

    object? GetValue();
}

public record Result<T>(T? Value, bool IsSuccess, string Error = "") : IAppResult
{
    public object? GetValue() => Value;

    public static Result<T> Success(T value) => new(value, true);
    public static Result<T> Failed(string error) => new(default, false, error);

    public static implicit operator Result<T>(T value) => Success(value);
}

public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failed<T>(string error) => Result<T>.Failed(error);
}
