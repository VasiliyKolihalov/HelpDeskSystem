using AutoMapper;
using SupportTickets.Api.Models.SupportTickets;
using SupportTickets.Api.Models.SupportTicketStatusRecords;

namespace SupportTickets.Api.Profiles;

public class SupportTicketStatusRecordsProfile : Profile
{
    public SupportTicketStatusRecordsProfile()
    {
        CreateMap<SupportTicketStatusRecord, SupportTicketStatusRecordView>();
        CreateMap<SupportTicket, SupportTicketStatusRecord>()
            .ForMember(
                destinationMember: record => record.SupportTicketId,
                memberOptions: options => options.MapFrom(supportTicket => supportTicket.Id));
    }
}