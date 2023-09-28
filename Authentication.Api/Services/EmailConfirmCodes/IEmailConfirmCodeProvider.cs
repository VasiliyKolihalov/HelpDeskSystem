namespace Authentication.Api.Services.EmailConfirmCodes;

public interface IEmailConfirmCodeProvider
{
    public string Generate();
}