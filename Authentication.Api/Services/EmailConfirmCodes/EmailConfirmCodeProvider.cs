namespace Authentication.Api.Services.EmailConfirmCodes;

public class EmailConfirmCodeProvider : IEmailConfirmCodeProvider
{
    public string Generate()
    {
        return new Random().Next(minValue: 10000, maxValue: 99999).ToString();
    }
}