using System.Text;
using SupportTickets.WebApi.Models.SupportTickets;

namespace SupportTickets.WebApi.Services;

public class SupportTicketsPaginationQueueBuilder : ISupportTicketsPaginationQueueBuilder
{
    private readonly StringBuilder _queue;
    private bool _thereIsFilter;

    public SupportTicketsPaginationQueueBuilder(string queue)
    {
        _queue = new StringBuilder(queue);
        _thereIsFilter = false;
    }

    public ISupportTicketsPaginationQueueBuilder WhereStatusEquals(SupportTicketStatus status)
    {
        if (_thereIsFilter)
        {
            _queue.Append($"and supporttickets.Status = '{status.ToString()}' ");
        }
        else
        {
            _queue.AppendLine($"where supporttickets.Status = '{status.ToString()}' ");
            _thereIsFilter = true;
        }

        return this;
    }

    public ISupportTicketsPaginationQueueBuilder WherePriorityEquals(SupportTicketPriority priority)
    {
        if (_thereIsFilter)
        {
            _queue.Append($"and supporttickets.Priority = '{priority.ToString()}' ");
        }
        else
        {
            _queue.AppendLine($"where supporttickets.Priority = '{priority.ToString()}' ");
            _thereIsFilter = true;
        }

        return this;
    }

    public ISupportTicketsPaginationQueueBuilder WhereAgentIdIsNull()
    {
        if (_thereIsFilter)
        {
            _queue.Append("and supporttickets.AgentId is null ");
        }
        else
        {
            _queue.AppendLine("where supporttickets.AgentId is null ");
            _thereIsFilter = true;
        }

        return this;
    }

    public ISupportTicketsPaginationQueueBuilder WhereUserIdOrAgentIdEquals(Guid id)
    {
        if (_thereIsFilter)
        {
            _queue.Append($"and (supportTickets.UserId = '{id}' or SupportTickets.AgentId = '{id}') ");
        }
        else
        {
            _queue.AppendLine($"where (SupportTickets.UserId = '{id}' or SupportTickets.AgentId = '{id}') ");
            _thereIsFilter = true;
        }

        return this;
    }

    public ISupportTicketsPaginationQueueBuilder LimitAndOffset(int limit, int offset)
    {
        _queue.AppendLine($"limit {limit} offset {offset} ");
        return this;
    }

    public string Build()
    {
        return _queue.ToString();
    }
}