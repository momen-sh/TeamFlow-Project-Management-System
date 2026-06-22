using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamFlow.Migrations
{
    /// <inheritdoc />
    public partial class TaskAttachmentMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "TaskAttachments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "TaskAttachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "TaskAttachments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "TaskAttachments");
        }
    }
}
