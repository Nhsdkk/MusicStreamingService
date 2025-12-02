using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions");

            migrationBuilder.RenameColumn(
                name: "PermissionId",
                table: "UserPermissions",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                newName: "IX_UserPermissions_RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Permissions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Permissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleEntityId",
                table: "Permissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoleEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleEntityId",
                table: "Permissions",
                column: "RoleEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_RoleEntity_RoleEntityId",
                table: "Permissions",
                column: "RoleEntityId",
                principalTable: "RoleEntity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_RoleEntity_RoleId",
                table: "UserPermissions",
                column: "RoleId",
                principalTable: "RoleEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_RoleEntity_RoleEntityId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_RoleEntity_RoleId",
                table: "UserPermissions");

            migrationBuilder.DropTable(
                name: "RoleEntity");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_RoleEntityId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "RoleEntityId",
                table: "Permissions");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "UserPermissions",
                newName: "PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPermissions_RoleId",
                table: "UserPermissions",
                newName: "IX_UserPermissions_PermissionId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Permissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Permissions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
