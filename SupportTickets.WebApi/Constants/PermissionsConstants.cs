namespace SupportTickets.WebApi.Constants;

public static class PermissionsConstants
{
    public static class SupportTicketsPermissions
    {
        private const string SupportTicket = "supporttickets";
        public const string GetAll = SupportTicket + ".getall";
        public const string GetById = SupportTicket + ".getbyid";
        public const string Update = SupportTicket + ".update";
        public const string Delete = SupportTicket + ".delete";

        public static readonly IEnumerable<string> PermissionsForPolicy = new[]
        {
            GetAll
        };
    }

    public static readonly IEnumerable<string> AllPermissionsForPolicy = SupportTicketsPermissions.PermissionsForPolicy;
}