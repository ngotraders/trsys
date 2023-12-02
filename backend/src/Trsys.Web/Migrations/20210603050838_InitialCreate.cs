using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Trsys.Web.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Streams",
                columns: table => new
                {
                    IdInternal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<string>(type: "char(42)", unicode: false, fixedLength: true, maxLength: 42, nullable: false),
                    IdOriginal = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((-1))"),
                    Position = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "((-1))"),
                    MaxAge = table.Column<int>(type: "int", nullable: true),
                    MaxCount = table.Column<int>(type: "int", nullable: true),
                    IdOriginalReversed = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, computedColumnSql: "(reverse([IdOriginal]))", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streams", x => x.IdInternal);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreamIdInternal = table.Column<int>(type: "int", nullable: false),
                    StreamVersion = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JsonMetadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Position);
                    table.ForeignKey(
                        name: "FK_Events_Streams",
                        column: x => x.StreamIdInternal,
                        principalTable: "Streams",
                        principalColumn: "IdInternal",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_StreamIdInternal_Created",
                table: "Messages",
                columns: new[] { "StreamIdInternal", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_StreamIdInternal_Id",
                table: "Messages",
                columns: new[] { "StreamIdInternal", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_StreamIdInternal_Revision",
                table: "Messages",
                columns: new[] { "StreamIdInternal", "StreamVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streams_Id",
                table: "Streams",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streams_IdOriginal",
                table: "Streams",
                columns: new[] { "IdOriginal", "IdInternal" });

            migrationBuilder.CreateIndex(
                name: "IX_Streams_IdOriginalReversed",
                table: "Streams",
                columns: new[] { "IdOriginalReversed", "IdInternal" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Streams");
        }
    }
}
