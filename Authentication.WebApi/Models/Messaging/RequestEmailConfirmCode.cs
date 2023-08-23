namespace Authentication.WebApi.Models.Messaging;

public class RequestEmailConfirmCode
{
    public string ConfirmCode { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
}