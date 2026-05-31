using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BE.Migrations
{
    /// <inheritdoc />
    public partial class restart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "tasks_table",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.CreateTable(
                name: "server_properties",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    totaltasks = table.Column<int>(type: "integer", nullable: false),
                    completedtasks = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_server_properties", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "server_properties");

            migrationBuilder.DropColumn(
                name: "status",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "tasks_table",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
