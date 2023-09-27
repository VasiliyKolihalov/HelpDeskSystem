using FluentMigrator;

namespace Authentication.Api.Migrations;

[Migration(20230819)]
public class Migration19082023 : Migration
{
    public override void Up()
    {
        Delete
            .FromTable("Permissions")
            .Row(new { Id = "supporttickets.agents.appoint" })
            .Row(new { Id = "accounts.getbyid" });

        Insert
            .IntoTable("Permissions")
            .Row(new { Id = "supporttickets.getfree" });

        Insert
            .IntoTable("RolesPermissions")
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.getfree" })
            .Row(new { RoleId = "agent", PermissionId = "supporttickets.getfree" });
    }

    public override void Down()
    {
        Insert
            .IntoTable("Permissions")
            .Row(new { Id = "supporttickets.agents.appoint" })
            .Row(new { Id = "accounts.getbyid" });

        Insert
            .IntoTable("RolesPermissions")
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.agents.appoint" })
            .Row(new { RoleId = "admin", PermissionId = "accounts.getbyid" });

        Delete
            .FromTable("Permissions")
            .Row(new { Id = "supporttickets.getfree" });
    }
}