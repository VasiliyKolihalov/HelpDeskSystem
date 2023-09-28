using FluentMigrator;

namespace Authentication.Api.Migrations;

[Migration(20230822)]
public class Migration22082023 : Migration
{
    public override void Up()
    {
        Create
            .Table("AccountsEmailConfirmCodes")
            .WithColumn("AccountId").AsGuid().PrimaryKey()
            .ForeignKey("Accounts", "Id")
            .WithColumn("Code").AsString().PrimaryKey()
            .WithColumn("DateTime").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("AccountsEmailConfirmCodes");
    }
}