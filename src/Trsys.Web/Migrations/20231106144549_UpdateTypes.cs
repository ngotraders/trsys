using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trsys.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update dbo.Messages set Type = replace(Type, 'Trsys.Web.Models', 'Trsys.Models')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update dbo.Messages set Type = replace(Type, 'Trsys.Models', 'Trsys.Web.Models')");
        }
    }
}
