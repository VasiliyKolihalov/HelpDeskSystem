namespace Infrastructure.Repositories;

public interface IRepository<TItem, in TId>
{
    public Task<IEnumerable<TItem>> GetAllAsync();
    public Task<TItem?> GetByIdAsync(TId id);
    public Task InsertAsync(TItem item);
    public Task UpdateAsync(TItem item);
    public Task DeleteAsync(TId id);
}