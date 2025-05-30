using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cyberjuice.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Employee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoiningDate",
                table: "CyberjuiceEmployees");

            migrationBuilder.DropColumn(
                name: "RemainingLeaveDays",
                table: "CyberjuiceEmployees");

            migrationBuilder.DropColumn(
                name: "TotalLeaveDays",
                table: "CyberjuiceEmployees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "JoiningDate",
                table: "CyberjuiceEmployees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RemainingLeaveDays",
                table: "CyberjuiceEmployees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLeaveDays",
                table: "CyberjuiceEmployees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
