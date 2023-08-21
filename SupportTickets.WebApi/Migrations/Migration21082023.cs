using System.Data;
using FluentMigrator;

namespace SupportTickets.WebApi.Migrations;

[Migration(20230821)]
public class Migration21082023 : Migration
{
    public override void Up()
    {
        Create
            .Table("SupportTicketStatusRecords")
            .WithColumn("SupportTicketId").AsGuid().PrimaryKey()
            .ForeignKey("SupportTickets", "Id").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Status").AsString().NotNullable().PrimaryKey()
            .WithColumn("DateTime").AsDateTime().PrimaryKey();
    }

    public override void Down()
    {
        Delete.Table("SupportTicketStatusRecords");
    }
}