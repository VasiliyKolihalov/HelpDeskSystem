using AutoMapper;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using Infrastructure.Exceptions;
using SupportTickets.Api.Constants;
using SupportTickets.Api.Models.Images;

namespace SupportTickets.Api.Services.Clients;

public class ResourcesGrpcClient : IResourcesGrpcClient
{
    private readonly Images.ImagesClient _imagesClient;
    private readonly IMapper _mapper;

    public ResourcesGrpcClient(GrpcClientFactory grpcClientFactory, IMapper mapper)
    {
        _mapper = mapper;
        _imagesClient = grpcClientFactory.CreateClient<Images.ImagesClient>(GrpcClientsNames.Images);
    }

    public async Task<IEnumerable<ImageView>?> SendGetMessageImagesRequestAsync(Guid messageId)
    {
        var request = new GetMessageImagesRequest
        {
            MessageId = messageId.ToString()
        };
        try
        {
            GetMessageImagesResponse response = await _imagesClient.GetByMessageIdAsync(request);
            return response.Images.Select(responseImage => _mapper.Map<ImageView>(responseImage)).ToList();
        }
        catch (RpcException exception)
        {
            if (exception.Status.StatusCode == StatusCode.NotFound)
                return null;

            throw;
        }
    }

    public async Task SendAddImagesToMessageRequestAsync(IEnumerable<ImageCreate> imageCreates, Guid messageId)
    {
        if (imageCreates == null) throw new ArgumentNullException(nameof(imageCreates));
        if (!imageCreates.Any())
            throw new ArgumentException("Value cannot be an empty collection.", nameof(imageCreates));

        var request = new AddToMessageImagesRequest
        {
            MessageId = messageId.ToString()
        };
        request.Images.AddRange(_mapper.Map<IEnumerable<ImageRequest>>(imageCreates));
        try
        {
            await _imagesClient.AddToMessageAsync(request);
        }
        catch (RpcException exception)
        {
            if (exception.Status.StatusCode == StatusCode.InvalidArgument)
                throw new BadRequestException(exception.Status.Detail);

            throw;
        }
    }
}