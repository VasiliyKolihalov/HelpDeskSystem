using AutoMapper;
using SupportTickets.Api.Models.Messages;

namespace SupportTickets.Api.Profiles;

public class MessagesProfile : Profile
{
    public MessagesProfile()
    {
        CreateMap<Message, MessageView>();
        CreateMap<MessageCreate, Message>();
        CreateMap<MessageUpdate, Message>();
    }
}