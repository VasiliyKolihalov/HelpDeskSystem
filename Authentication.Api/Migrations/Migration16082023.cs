using FluentMigrator;

namespace Authentication.Api.Migrations;

[Migration(20230816)]
public class Migration16082023 : Migration
{
    public override void Up()
    {
        Update
            .Table("Permissions")
            .Set(new {Id = "supporttickets.agents.appoint"})
            .Where(new { Id = "supporttickets.agents.set" });
    }

    public override void Down()
    {
        Update
            .Table("Permissions")
            .Set(new { Id = "supporttickets.agents.set" })
            .Where(new {Id = "supporttickets.agents.appoint"});
    }
}