using AutoMapper;
using SupportTickets.WebApi.Models.SupportTicketAgentRecords;
using SupportTickets.WebApi.Models.SupportTickets;

namespace SupportTickets.WebApi.Profiles;

public class SupportTicketAgentRecordsProfile : Profile
{
    public SupportTicketAgentRecordsProfile()
    {
        CreateMap<SupportTicketAgentRecord, SupportTicketAgentRecordView>();
        CreateMap<SupportTicket, SupportTicketAgentRecord>()
            .ForMember(
                destinationMember: record => record.SupportTicketId,
                memberOptions: options => options.MapFrom(supportTicket => supportTicket.Id));
    }
}