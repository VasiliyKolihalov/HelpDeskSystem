﻿namespace SupportTickets.Api.Models.Users;

public class UserView
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}