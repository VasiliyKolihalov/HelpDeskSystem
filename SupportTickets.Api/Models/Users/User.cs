﻿namespace SupportTickets.Api.Models.Users;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}