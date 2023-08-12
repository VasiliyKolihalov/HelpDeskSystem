using AutoMapper;
using SupportTickets.WebApi.Models.Messages;

namespace SupportTickets.WebApi.Profiles;

public class MessagesProfile : Profile
{
    public MessagesProfile()
    {
        CreateMap<Message, MessageView>();
        CreateMap<MessageCreate, Message>();
        CreateMap<MessageUpdate, Message>();
    }
}