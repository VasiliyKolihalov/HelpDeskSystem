using AutoMapper;
using SupportTickets.WebApi.Models.Images;
using SupportTickets.WebApi.Services;

namespace SupportTickets.WebApi.Profiles;

public class ImagesProfile : Profile
{
    public ImagesProfile()
    {
        CreateMap<ImageResponse, ImageView>();
        CreateMap<ImageCreate, ImageRequest>();
    }
}