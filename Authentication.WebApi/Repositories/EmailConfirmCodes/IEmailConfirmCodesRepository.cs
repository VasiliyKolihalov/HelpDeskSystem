using Authentication.WebApi.Models.EmailConfirmCodes;

namespace Authentication.WebApi.Repositories.EmailConfirmCodes;

public interface IEmailConfirmCodesRepository
{
    public Task<EmailConfirmCode?> GetByAccountIdAsync(Guid accountId);
    public Task<bool> IsExistsAsync(Guid accountId);
    public Task InsertAsync(EmailConfirmCode code);
    public Task DeleteByAccountIdAsync(Guid accountId);
}