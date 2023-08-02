namespace Authentication.WebApi.Services;

public class EmailConfirmCodeProvider : IEmailConfirmCodeProvider
{
    public string Generate()
    {
        return new Random().Next(maxValue: 5).ToString();
    }
}