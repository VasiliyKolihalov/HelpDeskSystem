using FluentMigrator;

namespace Authentication.WebApi.Migrations;

[Migration(20230821)]
public class Migration21082023 : Migration
{
    public override void Up()
    {
        Insert
            .IntoTable("Permissions")
            .Row(new { Id = "supporttickets.getagentshistory" })
            .Row(new { Id = "supporttickets.getstatushistory" });

        Insert
            .IntoTable("RolesPermissions")
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.getagentshistory" })
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.getstatushistory" });
    }

    public override void Down()
    {
        Delete
            .FromTable("Permissions")
            .Row(new { Id = "supporttickets.getagentshistory" })
            .Row(new { Id = "supporttickets.getstatushistory" });
    }
}