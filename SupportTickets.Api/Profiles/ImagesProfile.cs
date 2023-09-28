using AutoMapper;
using SupportTickets.Api.Models.Images;
using SupportTickets.Api.Services;

namespace SupportTickets.Api.Profiles;

public class ImagesProfile : Profile
{
    public ImagesProfile()
    {
        CreateMap<ImageResponse, ImageView>();
        CreateMap<ImageCreate, ImageRequest>();
    }
}