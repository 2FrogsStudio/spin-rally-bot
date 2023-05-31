using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpinRallyBot.Migrations
{
    /// <inheritdoc />
    public partial class FixPipelineState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PipelineStates",
                table: "PipelineStates");

            migrationBuilder.RenameTable(
                name: "PipelineStates",
                newName: "PipelineState");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PipelineState",
                table: "PipelineState",
                columns: new[] { "UserId", "ChatId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PipelineState",
                table: "PipelineState");

            migrationBuilder.RenameTable(
                name: "PipelineState",
                newName: "PipelineStates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PipelineStates",
                table: "PipelineStates",
                columns: new[] { "UserId", "ChatId" });
        }
    }
}
