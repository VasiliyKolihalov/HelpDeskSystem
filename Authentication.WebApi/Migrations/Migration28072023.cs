using FluentMigrator;

namespace Authentication.WebApi.Migrations;

[Migration(28072023)]
public class Migration28072023 : Migration
{
    public override void Up()
    {
        Alter
            .Table("Accounts")
            .AddColumn("IsEmailConfirm")
            .AsBoolean()
            .SetExistingRowsTo(false);

        Create
            .Table("AccountsEmailConfirmCodes")
            .WithColumn("AccountId").AsGuid().ForeignKey("Accounts", "Id").PrimaryKey()
            .WithColumn("ConfirmCode").AsString().ForeignKey();
    }

    public override void Down()
    {
        Delete
            .Column("IsEmailConfirm")
            .FromTable("Accounts");

        Delete.Table("AccountsEmailConfirmCodes");
    }
}