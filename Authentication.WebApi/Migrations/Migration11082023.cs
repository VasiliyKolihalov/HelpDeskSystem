using FluentMigrator;

namespace Authentication.WebApi.Migrations;

[Migration(20230811)]
public class Migration11082023 : Migration
{
    public override void Up()
    {
        Insert
            .IntoTable("Permissions")
            .Row(new { Id = "accounts.getbyid" })
            .Row(new { Id = "supporttickets.agents.set" });

        Insert
            .IntoTable("RolesPermissions")
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.agents.set" })
            .Row(new { RoleId = "admin", PermissionId = "accounts.getbyid" });

        Insert
            .IntoTable("Roles")
            .Row(new { Id = "agent" });
    }

    public override void Down()
    {
        Delete
            .FromTable("Permissions")
            .Row(new { Id = "supporttickets.agents.set" });

        Delete
            .FromTable("Roles")
            .Row(new { Id = "agent" });
    }
}