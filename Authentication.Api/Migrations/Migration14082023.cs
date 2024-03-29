using FluentMigrator;

namespace Authentication.Api.Migrations;

[Migration(20230814)]
public class Migration14082023 : Migration
{
    public override void Up()
    {
        Insert
            .IntoTable("Permissions")
            .Row(new { Id = "supporttickets.close" });

        Insert
            .IntoTable("RolesPermissions")
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.close" });
    }

    public override void Down()
    {
        Delete
            .FromTable("Permissions")
            .Row(new { Id = "supporttickets.close" });
    }
}