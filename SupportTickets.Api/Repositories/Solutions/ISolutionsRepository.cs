using SupportTickets.Api.Models.Solutions;

namespace SupportTickets.Api.Repositories.Solutions;

public interface ISolutionsRepository
{
    public Task InsertAsync(Solution solution);
    public Task UpdateAsync(Solution solution);
}