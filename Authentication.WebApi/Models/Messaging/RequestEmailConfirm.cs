namespace Authentication.WebApi.Models.Messaging;

public class RequestEmailConfirm
{
    public string ConfirmCode { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }

}