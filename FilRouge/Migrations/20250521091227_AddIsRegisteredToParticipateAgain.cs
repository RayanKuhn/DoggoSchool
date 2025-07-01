using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilRouge.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRegisteredToParticipateAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRegistered",
                table: "Participations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRegistered",
                table: "Participations");
        }
    }
}
