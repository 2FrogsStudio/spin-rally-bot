using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpinRallyBot.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePipelineState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PipelineStates",
                table: "PipelineStates");

            migrationBuilder.RenameColumn(
                name: "Command",
                table: "PipelineStates",
                newName: "Data");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "PipelineStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "PipelineStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PipelineStates",
                table: "PipelineStates",
                columns: new[] { "UserId", "ChatId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PipelineStates",
                table: "PipelineStates");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PipelineStates");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "PipelineStates");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "PipelineStates",
                newName: "Command");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PipelineStates",
                table: "PipelineStates",
                column: "Command");
        }
    }
}
