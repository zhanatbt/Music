namespace MusicApp.Application.Common;

public class OperationResult<T> : OperationResult
{
    public T? Data { get; private set; }

    public static OperationResult<T> Ok(T data, string message = "")
    {
        return new OperationResult<T>
        {
            Data = data,
            Success = true,
            Message = message
        };
    }

    public new static OperationResult<T> Fail(string message)
    {
        return new OperationResult<T>
        {
            Success = false,
            Message = message
        };
    }
}
