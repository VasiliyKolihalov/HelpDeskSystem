﻿namespace HelpDeskSystem.Models.SupportTicket;

public class SupportTicketView
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; }
}