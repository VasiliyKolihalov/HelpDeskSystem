namespace Authentication.WebApi.Models.EmailConfirmCodes;

public class EmailConfirmCode
{
    public Guid AccountId { get; set; }
    public string Code { get; set; }
    public DateTime DateTime { get; set; }
}