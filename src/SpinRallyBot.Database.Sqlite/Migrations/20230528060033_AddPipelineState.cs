using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpinRallyBot.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PipelineStates",
                columns: table => new
                {
                    Command = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineStates", x => x.Command);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PipelineStates");
        }
    }
}
