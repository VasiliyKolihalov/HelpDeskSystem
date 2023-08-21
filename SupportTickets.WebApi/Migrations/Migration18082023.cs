using System.Data;
using FluentMigrator;

namespace SupportTickets.WebApi.Migrations;

[Migration(20230818)]
public class Migration18082023 : Migration
{
    public override void Up()
    {
        Alter
            .Table("SupportTickets")
            .AddColumn("Priority").AsString().NotNullable();

        Create
            .Table("SupportTicketAgentRecords")
            .WithColumn("SupportTicketId").AsGuid().PrimaryKey()
            .ForeignKey("SupportTickets", "Id").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("AgentId").AsGuid().PrimaryKey()
            .ForeignKey("Users", "Id").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("DateTime").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Column("Priority").FromTable("SupportTickets");

        Delete.Table("SupportTicketAgentRecords");
    }
}