using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RightVisionBotDb.Migrations
{
    /// <inheritdoc />
    public partial class Teachers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantForm");

            migrationBuilder.CreateIndex(
                name: "IX_StudentForms_TeacherId",
                table: "StudentForms",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentForms_Teachers_TeacherId",
                table: "StudentForms",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentForms_Teachers_TeacherId",
                table: "StudentForms");

            migrationBuilder.DropIndex(
                name: "IX_StudentForms_TeacherId",
                table: "StudentForms");

            migrationBuilder.CreateTable(
                name: "ParticipantForm",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    CuratorId = table.Column<long>(type: "INTEGER", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Rate = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Telegram = table.Column<string>(type: "TEXT", nullable: false),
                    Track = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantForm", x => x.UserId);
                });
        }
    }
}
