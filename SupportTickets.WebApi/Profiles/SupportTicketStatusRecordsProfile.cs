using AutoMapper;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.SupportTicketStatusRecords;

namespace SupportTickets.WebApi.Profiles;

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