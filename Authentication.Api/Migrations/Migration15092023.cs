using FluentMigrator;

namespace Authentication.Api.Migrations;

[Migration(20230915)]
public class Migration15092023 : Migration
{
    public override void Up()
    {
        Insert
            .IntoTable("Permissions")
            .Row(new { Id = "accounts.getbyid" })
            .Row(new { Id = "accounts.delete" });

        Insert
            .IntoTable("RolesPermissions")
            .Row(new { RoleId = "admin", PermissionId = "accounts.getbyid" })
            .Row(new { RoleId = "admin", PermissionId = "accounts.delete" });
    }

    public override void Down()
    {
        Delete
            .FromTable("Permissions")
            .Row(new { Id = "accounts.getbyid" })
            .Row(new { Id = "accounts.delete" });
    }
}