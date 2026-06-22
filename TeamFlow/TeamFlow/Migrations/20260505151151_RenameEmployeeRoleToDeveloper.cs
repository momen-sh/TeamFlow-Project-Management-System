using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamFlow.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmployeeRoleToDeveloper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Users] SET [Role] = 'Developer' WHERE [Role] = 'Employee'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Users] SET [Role] = 'Employee' WHERE [Role] = 'Developer'");
        }
    }
}
