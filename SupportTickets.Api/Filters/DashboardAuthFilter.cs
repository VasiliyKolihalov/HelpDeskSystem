using Hangfire.Dashboard;

namespace SupportTickets.Api.Filters;

public class DashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Todo: Add jwt auth after frontend
        return true;
    }
}