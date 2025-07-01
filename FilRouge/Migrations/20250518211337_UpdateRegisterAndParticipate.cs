using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilRouge.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRegisterAndParticipate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebugTest",
                table: "Registers");

            migrationBuilder.DropColumn(
                name: "DebugTest",
                table: "Participations");

            migrationBuilder.AlterColumn<int>(
                name: "Note",
                table: "Registers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Participations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Note",
                table: "Registers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DebugTest",
                table: "Registers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Participations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DebugTest",
                table: "Participations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
