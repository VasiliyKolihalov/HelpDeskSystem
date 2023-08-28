using SupportTickets.WebApi.Models.Images;

namespace SupportTickets.WebApi.Services.Clients;

public interface IResourcesGrpcClient
{
    public Task<IEnumerable<ImageView>?> SendGetMessageImagesRequestAsync(Guid messageId);
    public Task SendAddImagesToMessageRequestAsync(IEnumerable<ImageCreate> imageCreates, Guid messageId);
}