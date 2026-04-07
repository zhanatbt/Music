using System.Text;
using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.Security;

public class PasswordValidator : IPasswordValidator
{
    public OperationResult Validate(string password)
    {
        password ??= string.Empty;

        var failedRules = new (string Error, bool IsValid)[]
        {
            ("Минимум 6 символов.", password.Length >= 6),
            ("Хотя бы одна заглавная буква.", password.Any(char.IsUpper)),
            ("Хотя бы одна строчная буква.", password.Any(char.IsLower)),
            ("Хотя бы один специальный символ.", password.Any(static c => !char.IsLetterOrDigit(c)))
        }
        .Where(rule => !rule.IsValid)
        .Select(rule => rule.Error)
        .ToArray();

        if (failedRules.Length == 0)
        {
            return OperationResult.Ok();
        }

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine("Пароль не соответствует требованиям:");

        foreach (var rule in failedRules)
        {
            messageBuilder.AppendLine($"- {rule}");
        }

        return OperationResult.Fail(messageBuilder.ToString());
    }
}
