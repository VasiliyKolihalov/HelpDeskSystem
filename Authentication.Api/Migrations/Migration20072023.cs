using System.Data;
using FluentMigrator;

namespace Authentication.Api.Migrations;

[Migration(20230720)]
public class Migration20072023 : Migration
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
            .WithColumn("AccountId").AsGuid()
            .ForeignKey("Accounts", "Id").OnDeleteOrUpdate(Rule.Cascade).PrimaryKey()
            .WithColumn("RoleId").AsString()
            .ForeignKey("Roles", "Id").OnDeleteOrUpdate(Rule.Cascade).PrimaryKey();

        Create
            .Table("Permissions")
            .WithColumn("Id").AsString().PrimaryKey();

        Create
            .Table("RolesPermissions")
            .WithColumn("RoleId").AsString()
            .ForeignKey("Roles", "Id").OnDeleteOrUpdate(Rule.Cascade).PrimaryKey()
            .WithColumn("PermissionId").AsString()
            .ForeignKey("Permissions", "Id").OnDeleteOrUpdate(Rule.Cascade).PrimaryKey();

        Insert
            .IntoTable("Accounts")
            .Row(new
            {
                Id = Guid.Parse("aa7621b8-bd5e-4195-8c60-72affb687caf"),
                Email = "admin@email.com",
                PasswordHash = "$2a$10$iFryqwvIedH5t7bXP9w2WOyTY78aetJq0vel5ri5sffdFmsmwUyNq"
            });

        Insert
            .IntoTable("Roles")
            .Row(new { Id = "admin" });

        Insert
            .IntoTable("AccountsRoles")
            .Row(new
            {
                AccountId = Guid.Parse("aa7621b8-bd5e-4195-8c60-72affb687caf"),
                RoleId = "admin"
            });

        Insert
            .IntoTable("Permissions")
            .Row(new { Id = "accounts.addtorole" })
            .Row(new { Id = "accounts.removefromrole" })
            .Row(new { Id = "roles.create" })
            .Row(new { Id = "roles.update" })
            .Row(new { Id = "roles.delete" })
            .Row(new { Id = "users.create" })
            .Row(new { Id = "users.update" })
            .Row(new { Id = "users.delete" })
            .Row(new { Id = "supporttickets.getall" })
            .Row(new { Id = "supporttickets.getbyid" })
            .Row(new { Id = "supporttickets.update" })
            .Row(new { Id = "supporttickets.delete" });

        Insert
            .IntoTable("RolesPermissions")
            .Row(new { RoleId = "admin", PermissionId = "accounts.addtorole" })
            .Row(new { RoleId = "admin", PermissionId = "accounts.removefromrole" })
            .Row(new { RoleId = "admin", PermissionId = "roles.create" })
            .Row(new { RoleId = "admin", PermissionId = "roles.update" })
            .Row(new { RoleId = "admin", PermissionId = "roles.delete" })
            .Row(new { RoleId = "admin", PermissionId = "users.create" })
            .Row(new { RoleId = "admin", PermissionId = "users.update" })
            .Row(new { RoleId = "admin", PermissionId = "users.delete" })
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.getall" })
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.getbyid" })
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.update" })
            .Row(new { RoleId = "admin", PermissionId = "supporttickets.delete" });
    }

    public override void Down()
    {
        Delete
            .FromTable("RolesPermissions")
            .Row(new { RoleId = "admin", PermissionId = "accounts.addtorole" })
            .Row(new { RoleId = "admin", PermissionId = "accounts.removefromrole" })
            .Row(new { RoleId = "admin", PermissionId = "roles.create" })
            .Row(new { RoleId = "admin", PermissionId = "roles.update" })
            .Row(new { RoleId = "admin", PermissionId = "roles.delete" });

        Delete
            .FromTable("Permissions")
            .Row(new { Id = "accounts.addtorole" })
            .Row(new { Id = "accounts.removefromrole" })
            .Row(new { Id = "roles.create" })
            .Row(new { Id = "roles.update" })
            .Row(new { Id = "roles.delete" });

        Delete
            .FromTable("AccountsRoles")
            .Row(new
            {
                AccountId = Guid.Parse("aa7621b8-bd5e-4195-8c60-72affb687caf"),
                RoleId = "admin"
            });

        Delete
            .FromTable("Roles")
            .Row(new { Id = "admin" });

        Delete
            .FromTable("Accounts")
            .Row(new { Id = Guid.Parse("aa7621b8-bd5e-4195-8c60-72affb687caf") });

        Delete.Table("RolesPermissions");
        Delete.Table("Permissions");
        Delete.Table("AccountsRoles");
        Delete.Table("Roles");
        Delete.Table("Accounts");
    }
}