using Resources.WebApi.Models.Images;

namespace Resources.WebApi.Repositories;

public interface IImagesRepository
{
    public Task<IEnumerable<Image>> GetByMessageIdAsync(Guid messageId);
    public Task InsertAsync(Image image);
    public Task AddToMessageAsync(Guid imageId, Guid messageId);
}