namespace Authentication.Api.Models.EmailConfirmCodes;

public class EmailConfirmCode
{
    public Guid AccountId { get; set; }
    public string Code { get; set; }
    public DateTime DateTime { get; set; }
}