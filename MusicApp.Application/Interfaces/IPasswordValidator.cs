using MusicApp.Application.Common;

namespace MusicApp.Application.Interfaces;

public interface IPasswordValidator
{
    OperationResult Validate(string password);
}
