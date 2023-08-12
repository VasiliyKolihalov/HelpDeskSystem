using System.Data;
using FluentMigrator;

namespace SupportTickets.WebApi.Migrations;

[Migration(20230810)]
public class Migration10082023 : Migration
{
    public override void Up()
    {
        Alter
            .Table("SupportTickets")
            .AddColumn("AgentId").AsGuid()
            .ForeignKey("Users", "Id")
            .SetExistingRowsTo(null);

        Create.Table("Messages")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("SupportTicketId").AsGuid().NotNullable()
            .ForeignKey("SupportTickets", "Id").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("UserId").AsGuid().NotNullable()
            .ForeignKey("Users", "Id").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Content").AsString(1000).NotNullable();
    }

    public override void Down()
    {
        Delete.Column("AgentId").FromTable("SupportTickets");
        Delete.Table("Messages");
    }
}