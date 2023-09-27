using SupportTickets.Api.Models.Images;

namespace SupportTickets.Api.Services.Clients;

public interface IResourcesGrpcClient
{
    public Task<IEnumerable<ImageView>?> SendGetMessageImagesRequestAsync(Guid messageId);
    public Task SendAddImagesToMessageRequestAsync(IEnumerable<ImageCreate> imageCreates, Guid messageId);
}