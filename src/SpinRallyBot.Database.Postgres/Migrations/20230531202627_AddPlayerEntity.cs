using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpinRallyBot.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerUrl = table.Column<string>(type: "text", nullable: false),
                    Fio = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerUrl);
                });
            
            migrationBuilder.Sql("""
                INSERT INTO "Players"
                SELECT DISTINCT "PlayerUrl", "Fio"                 
                FROM "Subscriptions";
            """);
            
            migrationBuilder.DropColumn(
                name: "Fio",
                table: "Subscriptions");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlayerUrl",
                table: "Subscriptions",
                column: "PlayerUrl");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Players_PlayerUrl",
                table: "Subscriptions",
                column: "PlayerUrl",
                principalTable: "Players",
                principalColumn: "PlayerUrl",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Players_PlayerUrl",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PlayerUrl",
                table: "Subscriptions");

            migrationBuilder.AddColumn<string>(
                name: "Fio",
                table: "Subscriptions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
