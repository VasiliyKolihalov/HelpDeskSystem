using AutoMapper;
using Resources.WebApi.Models.Images;
using Resources.WebApi.Services;

namespace Resources.WebApi.Profiles;

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