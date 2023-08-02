namespace Authentication.WebApi.Services;

public interface IEmailConfirmCodeProvider
{
    public string Generate();
}