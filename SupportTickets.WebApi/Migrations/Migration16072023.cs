﻿using System.Data;
using FluentMigrator;

namespace SupportTickets.WebApi.Migrations;

[Migration(16072023)]
public class Migration16072023 : Migration
{
    public override void Up()
    {
        Create
            .Table("Users")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Firstname").AsString(150).NotNullable()
            .WithColumn("Lastname").AsString(150).NotNullable();

        Create.Table("SupportTickets")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Description").AsString(500).NotNullable()
            .WithColumn("UserId").AsGuid().ForeignKey("Users", "Id").OnDeleteOrUpdate(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.Table("SupportTickets");
        Delete.Table("Users");
    }
}