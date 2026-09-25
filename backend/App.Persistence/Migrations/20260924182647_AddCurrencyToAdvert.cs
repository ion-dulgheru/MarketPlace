using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Persistence.Migrations
{
    public partial class AddCurrencyToAdvert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Adverts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Mdl");

            migrationBuilder.Sql(
                """
                UPDATE "Adverts"
                SET "Currency" = 'Eur'
                WHERE "UserUuid" = 'b53f41a4-631e-43d9-ad05-988d5ddd48f3';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Adverts");
        }
    }
}
