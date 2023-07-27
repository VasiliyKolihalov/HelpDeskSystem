using System.Data;
using FluentMigrator;

namespace Authentication.WebApi.Migrations;

[Migration(19072023)]
public class Migration19072023 : Migration
{
    public override void Up()
    {
        Create
            .Table("Accounts")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Email").AsString(150)
            .WithColumn("PasswordHash").AsString(500);

        Create
            .Table("Roles")
            .WithColumn("Id").AsString().PrimaryKey();

        Create
            .Table("AccountsRoles")
            .WithColumn("AccountId").AsGuid().ForeignKey("Accounts", "Id").OnDeleteOrUpdate(Rule.Cascade).PrimaryKey()
            .WithColumn("RoleId").AsString().ForeignKey("Roles", "Id").OnDeleteOrUpdate(Rule.Cascade).PrimaryKey();

        Create
            .Table("Permissions")
            .WithColumn("Id").AsString().PrimaryKey();

        Create
            .Table("RolesPermissions")
            .WithColumn("RoleId").AsString().ForeignKey("Roles", "Id").OnDeleteOrUpdate(Rule.Cascade).PrimaryKey()
            .WithColumn("PermissionId").AsString().ForeignKey("Permissions", "Id").OnDeleteOrUpdate(Rule.Cascade)
            .PrimaryKey();
    }

    public override void Down()
    {
        Delete.Table("RolesPermissions");
        Delete.Table("Permissions");
        Delete.Table("AccountsRoles");
        Delete.Table("Roles");
        Delete.Table("Accounts");
    }
}