using AutoMapper;
using Grpc.Core;
using Infrastructure.Exceptions;
using Resources.WebApi.Models.Images;
using Resources.WebApi.Services;

namespace Resources.WebApi.GrpcServices;

public class GrpcImagesService : Images.ImagesBase
{
    private readonly ImagesService _imagesService;
    private readonly IMapper _mapper;

    public GrpcImagesService(
        ImagesService imagesService,
        IMapper mapper)
    {
        _imagesService = imagesService;
        _mapper = mapper;
    }

    public override async Task<GetMessageImagesResponse> GetByMessageId(
        GetMessageImagesRequest request,
        ServerCallContext _)
    {
        Guid messageId = Guid.Parse(request.MessageId);
        IEnumerable<ImageView> imageViews;
        try
        {
            imageViews = await _imagesService.GetByMessageIdAsync(messageId);
        }
        catch (NotFoundException exception)
        {
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
        var response = new GetMessageImagesResponse();
        response.Images.AddRange(_mapper.Map<IEnumerable<ImageResponse>>(imageViews));
        return response;
    }

    public override async Task<Empty> AddToMessage(AddToMessageImagesRequest request, ServerCallContext _)
    {
        Guid messageId = Guid.Parse(request.MessageId);
        var imageCreates = _mapper.Map<IEnumerable<ImageCreate>>(request.Images);
        try
        {
            await _imagesService.AddToMessageAsync(messageId, imageCreates);
        }
        catch (BadRequestException exception)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, exception.Message));
        }
        return new Empty();
    }
}