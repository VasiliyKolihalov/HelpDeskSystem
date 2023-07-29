using FluentMigrator;

namespace Users.WebApi.Migrations;

[Migration(16072023)]
public class Migration16072023 : Migration
{
    public override void Up()
    {
        Create
            .Table("Users")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Firstname").AsString(150).NotNullable()
            .WithColumn("Lastname").AsString(150).NotNullable()
            .WithColumn("Email").AsString(150).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}