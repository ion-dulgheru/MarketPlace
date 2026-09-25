using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToAdvert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Adverts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Mdl");

            // One-time data correction: the 30 apartamente_ro.json listings imported via
            // /api/admin/adverts/import were priced in EUR (source: makler.md), not MDL.
            migrationBuilder.Sql(
                """
                UPDATE "Adverts"
                SET "Currency" = 'Eur'
                WHERE "UserUuid" = 'b53f41a4-631e-43d9-ad05-988d5ddd48f3';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Adverts");
        }
    }
}
