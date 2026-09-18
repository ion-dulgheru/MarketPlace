using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingTypeAndCategoryDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApartmentBlock",
                table: "Adverts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApartmentFloor",
                table: "Adverts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApartmentNumber",
                table: "Adverts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingType",
                table: "Adverts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Apartment");

            migrationBuilder.AddColumn<decimal>(
                name: "GardenSquareMeters",
                table: "Adverts",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Levels",
                table: "Adverts",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApartmentBlock",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "ApartmentFloor",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "ApartmentNumber",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "BuildingType",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "GardenSquareMeters",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "Levels",
                table: "Adverts");
        }
    }
}
