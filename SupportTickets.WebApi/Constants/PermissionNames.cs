namespace SupportTickets.WebApi.Constants;

public static class PermissionNames
{
    public static class SupportTicketPermissions
    {
        private const string SupportTicket = "supporttickets";
        public const string GetAll = SupportTicket + ".getall";
        public const string GetById = SupportTicket + ".getbyid";
        public const string Update = SupportTicket + ".update";
        public const string Delete = SupportTicket + ".delete";

        private const string Agents = "agents";
        public const string SetAgent = SupportTicket + "." + Agents + ".set";

        public static readonly IEnumerable<string> PolicyPermissions = new[]
        {
            GetAll, SetAgent
        };
    }

    public static class HttpClientPermissions
    {
        public const string AccountsGetById = "accounts.getbyid";
    }

    public static readonly IEnumerable<string> AllPolicyPermissions = SupportTicketPermissions.PolicyPermissions;
}