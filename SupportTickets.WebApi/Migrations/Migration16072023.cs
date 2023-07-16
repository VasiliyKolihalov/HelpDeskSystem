using FluentMigrator;

namespace SupportTickets.WebApi.Migrations;

[Migration(16072023)]
public class Migration16072023 : Migration
{
    public override void Up()
    {
        Create
            .Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("firstname").AsString(150).NotNullable()
            .WithColumn("lastname").AsString(150).NotNullable();

        Create.Table("supporttickets")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("description").AsString(500).NotNullable()
            .WithColumn("userid").AsGuid().ForeignKey("users", "id");
    }

    public override void Down()
    {
        Delete.Table("supporttickets");
        Delete.Table("users");
    }
}