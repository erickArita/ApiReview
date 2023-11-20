using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiReview.Migrations
{
    /// <inheritdoc />
    public partial class AddCampoFotoEnAutorYPortadaEnBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Portada",
                schema: "transacctional",
                table: "books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Foto",
                schema: "transacctional",
                table: "autores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Portada",
                schema: "transacctional",
                table: "books");

            migrationBuilder.DropColumn(
                name: "Foto",
                schema: "transacctional",
                table: "autores");
        }
    }
}
