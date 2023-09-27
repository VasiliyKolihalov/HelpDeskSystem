using FluentMigrator;

namespace Authentication.Api.Migrations;

[Migration(20230728)]
public class Migration28072023 : Migration
{
    public override void Up()
    {
        Alter
            .Table("Accounts")
            .AddColumn("IsEmailConfirm").AsBoolean().NotNullable().SetExistingRowsTo(false);
    }

    public override void Down()
    {
        Delete
            .Column("IsEmailConfirm")
            .FromTable("Accounts");
    }
}