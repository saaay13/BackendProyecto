using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendProyecto.Migrations
{
    /// <inheritdoc />
    public partial class estado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioRol_Rol_IdRol",
                table: "UsuarioRol");

            migrationBuilder.AddColumn<string>(
                name: "UrlCertificado",
                table: "Certificado",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlCarnet",
                table: "Carnet",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioRol_Rol_IdRol",
                table: "UsuarioRol",
                column: "IdRol",
                principalTable: "Rol",
                principalColumn: "IdRol",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioRol_Rol_IdRol",
                table: "UsuarioRol");

            migrationBuilder.DropColumn(
                name: "UrlCertificado",
                table: "Certificado");

            migrationBuilder.DropColumn(
                name: "UrlCarnet",
                table: "Carnet");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioRol_Rol_IdRol",
                table: "UsuarioRol",
                column: "IdRol",
                principalTable: "Rol",
                principalColumn: "IdRol",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
