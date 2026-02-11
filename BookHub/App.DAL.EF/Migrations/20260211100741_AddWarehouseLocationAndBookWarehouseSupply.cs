using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace App.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseLocationAndBookWarehouseSupply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Warehouses",
                type: "geometry",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "BooksWarehouses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSupply",
                table: "BooksWarehouses",
                type: "timestamp with time zone",
                nullable: true);

            // Backfill Location from existing GpsX (latitude) / GpsY (longitude)
            migrationBuilder.Sql(@"
                UPDATE ""Warehouses""
                SET ""Location"" = ST_SetSRID(ST_MakePoint(""GpsY"", ""GpsX""), 4326)
                WHERE ""Location"" IS NULL;
            ");

            // Backfill Count (1–15) and LastSupply (random within last 90 days)
            migrationBuilder.Sql(@"
                UPDATE ""BooksWarehouses""
                SET ""Count""      = floor(random() * 15 + 1)::int,
                    ""LastSupply"" = NOW() - (floor(random() * 91) || ' days')::interval
                WHERE ""Count"" = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "BooksWarehouses");

            migrationBuilder.DropColumn(
                name: "LastSupply",
                table: "BooksWarehouses");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
