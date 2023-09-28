using AutoMapper;
using Resources.Api.Models.Images;
using Resources.Api.Services;

namespace Resources.Api.Profiles;

public class ImageProfile : Profile
{
    public ImageProfile()
    {
        CreateMap<Image, ImageView>()
            .ForMember(
                _ => _.Base64Content, 
                expression => expression.MapFrom(_ => Convert.ToBase64String(_.Content)));
        CreateMap<ImageView, ImageResponse>();
        CreateMap<ImageRequest, ImageCreate>();
    }
}