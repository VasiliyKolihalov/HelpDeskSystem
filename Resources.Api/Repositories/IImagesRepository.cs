using Resources.Api.Models.Images;

namespace Resources.Api.Repositories;

public interface IImagesRepository
{
    public Task<IEnumerable<Image>> GetByMessageIdAsync(Guid messageId);
    public Task InsertAsync(Image image);
    public Task AddToMessageAsync(Guid imageId, Guid messageId);
}