using System.ComponentModel.DataAnnotations;

namespace Authentication.Infrastructure.Attributes;

public class PasswordAttribute : ValidationAttribute
{
    private readonly int _minLenght;
    private readonly int _maxLenght;
    private readonly bool _anyUpperCharacterRequired;
    private readonly bool _anyLowerCharacterRequired;
    private readonly bool _noSpaces;
    private readonly bool _anySpecialCharactersRequired;

    public PasswordAttribute(
        int minLenght = 8,
        int maxLenght = 16,
        bool anyUpperCharacterRequired = true,
        bool anyLowerCharacterRequired = true,
        bool noSpaces = true,
        bool anySpecialCharactersRequired = true)
    {
        _minLenght = minLenght;
        _maxLenght = maxLenght;
        _anyUpperCharacterRequired = anyUpperCharacterRequired;
        _anyLowerCharacterRequired = anyLowerCharacterRequired;
        _noSpaces = noSpaces;
        _anySpecialCharactersRequired = anySpecialCharactersRequired;
    }

    public override bool IsValid(object? value)
    {
        if (value is not string password)
            throw new ValidationException("PasswordAttribute meant for strings");

        var isValid = true;

        if (password.Length < _minLenght || password.Length > _maxLenght)
        {
            ErrorMessage = $"Password length must be between {_minLenght} and {_maxLenght} characters";
            isValid = false;
        }

        if (_anyUpperCharacterRequired && !password.Any(char.IsUpper))
        {
            ErrorMessage = "Password must contain at least 1 capital letter";
            isValid = false;
        }

        if (_anyLowerCharacterRequired && !password.Any(char.IsLower))
        {
            ErrorMessage = "Password must contain at least 1 uppercase letter";
            isValid = false;
        }

        if (_noSpaces && password.Contains(' '))
        {
            ErrorMessage = "Password must not include spaces";
            isValid = false;
        }

        const string specialCharacters = @"%!@#$%^&*()?/>.<,:;'\|}]{[_~`+=-" + "\"";
        if (_anySpecialCharactersRequired && !specialCharacters.Any(password.Contains))
        {
            ErrorMessage = "Password must contain special characters";
            isValid = false;
        }

        return isValid;
    }
}