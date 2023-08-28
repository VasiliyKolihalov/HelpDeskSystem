using Microsoft.EntityFrameworkCore;
using Resources.WebApi.Models.Images;

namespace Resources.WebApi.Repositories;

public class ImagesRepository : IImagesRepository
{
    private readonly ApplicationContext _context;

    public ImagesRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Image>> GetByMessageIdAsync(Guid messageId)
    {
        return await _context.ImagesMessages
            .Where(_ => _.MessageId == messageId)
            .Include(_ => _.Image)
            .Select(_ => _.Image)
            .ToListAsync();
    }

    public async Task InsertAsync(Image image)
    {
        _context.Images.Add(image);
        await _context.SaveChangesAsync();
    }

    public async Task AddToMessageAsync(Guid imageId, Guid messageId)
    {
        var imageMessage = new ImageMessage
        {
            MessageId = messageId,
            ImageId = imageId
        };
        _context.ImagesMessages.Add(imageMessage);
        await _context.SaveChangesAsync();
    }
}