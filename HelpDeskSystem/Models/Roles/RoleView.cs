﻿namespace HelpDeskSystem.Models.Roles;

public class RoleView
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public List<string> Permissions { get; set; }
}