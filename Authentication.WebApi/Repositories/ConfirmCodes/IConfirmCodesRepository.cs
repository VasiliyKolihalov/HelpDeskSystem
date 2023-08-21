namespace Authentication.WebApi.Repositories.ConfirmCodes;

public interface IConfirmCodesRepository
{
    public Task<string?> GetByAccountIdAsync(Guid accountId);
    public Task<bool> IsExistsAsync(Guid accountId);
    public Task InsertAsync(Guid accountId, string code);
    public Task DeleteAsync(Guid accountId);
}