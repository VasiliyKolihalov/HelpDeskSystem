using System.Data;
using FluentMigrator;

namespace SupportTickets.WebApi.Migrations;

[Migration(20230816)]
public class Migration16082023 : Migration
{
    public override void Up()
    {
        Delete.Column("IsClose").FromTable("SupportTickets");

        Alter
            .Table("SupportTickets")
            .AddColumn("Status").AsString().NotNullable();

        Create
            .Table("Solutions")
            .WithColumn("MessageId").AsGuid().PrimaryKey()
            .ForeignKey("Messages", "Id").OnDeleteOrUpdate(Rule.Cascade)
            .WithColumn("Status").AsString().NotNullable();
    }

    public override void Down()
    {
        Alter
            .Table("SupportTickets")
            .AddColumn("IsClose").AsBoolean().Nullable();
        
        Delete.Column("Status").FromTable("SupportTickets");

        Delete.Table("Solutions");
    }
}