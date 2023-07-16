using FluentMigrator;

namespace Users.WebApi.Migrations;

[Migration(16072023)]
public class Migration16072023 : Migration
{
    public override void Up()
    {
        Create
            .Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("firstname").AsString(150).NotNullable()
            .WithColumn("lastname").AsString(150).NotNullable()
            .WithColumn("email").AsString(150).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("users");
    }
}