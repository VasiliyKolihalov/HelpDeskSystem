# HelpDeskSystem

## What is this?

### - This is ASP.NET Core backend application.

## What is the purpose?

### - Create simple system for processing service processes, technical support, which helps service companies reduce the cost of services and customer churn, as well as improve the quality of work performed.

## What architecture?

### - Monolith

## What subd?

### - Microsoft SQL Server

## What libraries are used?

- Dapper
- Newtonsoft.json
- AutoMapper

## Planned/Completed features:

- [x] Customers open support tickets describing issues they are facing.
- [ ] Both the customer and the support agent append messages, and all the correspondence is tracked by the support
  ticket.
- [ ] Each ticket has a priority: low, medium, high, or urgent.
- [ ] An agent should offer a solution within a set time limit (SLA) that is based on the ticket's priority.
- [ ] If the agent doesn't reply within the SLA, the customer can escalate the ticket to the agent's manager.
- [ ] Escalation reduces the agent's response time limit by 33%.
- [ ] If the agent didn't open an escalated ticket within 50% of the response time limit, it is automatically reassigned
  to a different agent.
- [ ] Tickets are automatically closed if the customer doesn't reply to the agent's questions within seven days.
- [ ] Escalated tickets cannot be closed automatically or by the agent, only by the cus- tomer or the agent's manager.
- [ ] A customer can reopen a closed ticket only if it was closed in the past seven days.
