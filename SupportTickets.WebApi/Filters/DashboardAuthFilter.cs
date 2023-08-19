using Hangfire.Dashboard;

namespace SupportTickets.WebApi.Filters;

public class DashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Todo: Add jwt auth after frontend
        return true;
    }
}