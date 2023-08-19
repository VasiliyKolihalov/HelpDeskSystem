namespace SupportTickets.WebApi.Constants;

public static class PermissionNames
{
    public static class SupportTicketPermissions
    {
        private const string SupportTicket = "supporttickets";
        public const string GetAll = SupportTicket + ".getall";
        public const string GetFree = SupportTicket + ".getfree";
        public const string GetById = SupportTicket + ".getbyid";
        public const string Update = SupportTicket + ".update";
        public const string Delete = SupportTicket + ".delete";
        public const string Close = SupportTicket + ".close";

        public static readonly IEnumerable<string> PolicyPermissions = new[]
        {
            GetAll, GetFree
        };
    }

    public static readonly IEnumerable<string> AllPolicyPermissions = SupportTicketPermissions.PolicyPermissions;
}