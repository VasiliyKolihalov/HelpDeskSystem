using Authentication.Api.Models.EmailConfirmCodes;

namespace Authentication.Api.Repositories.EmailConfirmCodes;

public interface IEmailConfirmCodesRepository
{
    public Task<EmailConfirmCode?> GetByAccountIdAsync(Guid accountId);
    public Task<bool> IsExistsAsync(Guid accountId);
    public Task InsertAsync(EmailConfirmCode code);
    public Task DeleteByAccountIdAsync(Guid accountId);
}