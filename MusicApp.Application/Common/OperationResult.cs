namespace MusicApp.Application.Common;

public class OperationResult
{
    public bool Success { get; protected set; }
    public string Message { get; protected set; } = string.Empty;

    public static OperationResult Ok(string message = "") => new() { Success = true, Message = message };
    public static OperationResult Fail(string message) => new() { Success = false, Message = message };
}
