using FluentMigrator;

namespace SupportTickets.Api.Migrations;

[Migration(20230919)]
public class Migration19092023 : Migration
{
    public override void Up()
    {
        Alter
            .Table("Messages")
            .AddColumn("DateTime")
            .AsDateTime()
            .NotNullable();
    }

    public override void Down()
    {
        Delete.Column("DateTime").FromTable("Messages");
    }
}