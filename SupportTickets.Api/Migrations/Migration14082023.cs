using FluentMigrator;

namespace SupportTickets.Api.Migrations;

[Migration(20230814)]
public class Migration14082023 : Migration
{
    public override void Up()
    {
        Alter
            .Table("SupportTickets")
            .AddColumn("IsClose").AsBoolean().WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete.Column("IsClose").FromTable("SupportTickets");
    }
}