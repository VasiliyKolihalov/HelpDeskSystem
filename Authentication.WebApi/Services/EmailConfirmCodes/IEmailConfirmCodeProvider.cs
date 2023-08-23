namespace Authentication.WebApi.Services.EmailConfirmCodes;

public interface IEmailConfirmCodeProvider
{
    public string Generate();
}