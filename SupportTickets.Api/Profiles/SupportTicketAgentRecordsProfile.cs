using AutoMapper;
using SupportTickets.Api.Models.SupportTicketAgentRecords;
using SupportTickets.Api.Models.SupportTickets;

namespace SupportTickets.Api.Profiles;

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