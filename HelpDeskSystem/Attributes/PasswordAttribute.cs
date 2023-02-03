using System.ComponentModel.DataAnnotations;

namespace HelpDeskSystem.Attributes;

public class PasswordAttribute : ValidationAttribute
{
    private readonly int _minLenght;
    private readonly int _maxLenght;
    private readonly bool _requiredUpper;
    private readonly bool _requiredLover;
    private readonly bool _requiredSpecialCharacters;

    public PasswordAttribute(
        int minLenght = 8,
        int maxLenght = 16,
        bool requiredUpper = true,
        bool requiredLover = true,
        bool requiredSpecialCharacters = true)
    {
        _minLenght = minLenght;
        _maxLenght = maxLenght;
        _requiredUpper = requiredUpper;
        _requiredLover = requiredLover;
        _requiredSpecialCharacters = requiredSpecialCharacters;
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
            throw new ValidationException("The Password is required");
        
        if (value is not string password)
            throw new ValidationException("Password is not string");

        var isValid = true;

        if (password.Length < _minLenght || password.Length > _maxLenght)
        {
            ErrorMessage = $"Password length must be between {_minLenght} and {_maxLenght} characters";
            isValid = false;
        }

        if (_requiredUpper && !password.Any(char.IsUpper))
        {
            ErrorMessage = "Password must contain at least 1 capital letter";
            isValid = false;
        }

        if (_requiredLover && !password.Any(char.IsLower))
        {
            ErrorMessage = "Password must contain at least 1 uppercase letter";
            isValid = false;
        }

        const string specialCharacters = @"%!@#$%^&*()?/>.<,:;'\|}]{[_~`+=-" + "\"";
        if (_requiredSpecialCharacters && !specialCharacters.Any(password.Contains))
        {
            ErrorMessage = "Password must contain at least special characters";
            isValid = false;
        }

        if (!password.Contains(' ')) return isValid;
        ErrorMessage = "Password must not include spaces";
        isValid = false;

        return isValid;
    }
}