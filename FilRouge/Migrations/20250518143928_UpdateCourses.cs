using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilRouge.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Courses");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "Dogs",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DurationInMinutes",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsValidatedByAdmin",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInMinutes",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsValidatedByAdmin",
                table: "Courses");

            migrationBuilder.AlterColumn<int>(
                name: "Weight",
                table: "Dogs",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Duration",
                table: "Courses",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }
    }
}
