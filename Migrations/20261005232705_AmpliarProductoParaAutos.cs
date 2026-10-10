using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPrimera.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarProductoParaAutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Producto",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Producto",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Anio",
                table: "Producto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Producto",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Producto",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Combustible",
                table: "Producto",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Destacado",
                table: "Producto",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Kilometraje",
                table: "Producto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Producto",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioOriginal",
                table: "Producto",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transmision",
                table: "Producto",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anio",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Combustible",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Destacado",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Kilometraje",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "PrecioOriginal",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "Transmision",
                table: "Producto");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Producto",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(120)",
                oldMaxLength: 120)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Producto",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
