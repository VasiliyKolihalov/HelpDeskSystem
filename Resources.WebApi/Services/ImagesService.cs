using AutoMapper;
using Infrastructure.Exceptions;
using Resources.WebApi.Models.Images;
using Resources.WebApi.Repositories;

namespace Resources.WebApi.Services;

public class ImagesService
{
    private readonly IImagesRepository _imagesRepository;
    private readonly IMapper _mapper;

    public ImagesService(IImagesRepository imagesRepository, IMapper mapper)
    {
        _imagesRepository = imagesRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ImageView>> GetByMessageIdAsync(Guid messageId)
    {
        IEnumerable<Image> images = await _imagesRepository.GetByMessageIdAsync(messageId);
        if (!images.Any())
            throw new NotFoundException($"Images for message with id: {messageId} not found");
        return _mapper.Map<IEnumerable<ImageView>>(images);
    }

    public async Task AddToMessageAsync(Guid messageId, IEnumerable<ImageCreate> imageCreates)
    {
        foreach (ImageCreate imageCreate in imageCreates)
        {
            var image = new Image
            {
                Id = Guid.NewGuid()
            };
            try
            {
                image.Content = Convert.FromBase64String(imageCreate.Base64Content);
            }
            catch (FormatException)
            {
                throw new BadRequestException("Wrong base64 image content");
            }

            await _imagesRepository.InsertAsync(image);
            await _imagesRepository.AddToMessageAsync(image.Id, messageId);
        }
    }
}