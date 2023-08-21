using SupportTickets.WebApi.Models.Solutions;

namespace SupportTickets.WebApi.Repositories.Solutions;

public interface ISolutionsRepository
{
    public Task InsertAsync(Solution solution);
    public Task UpdateAsync(Solution solution);
}