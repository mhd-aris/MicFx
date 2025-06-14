using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicFx.Modules.Auth.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTablePrefixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_Roles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Auth_Users");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "Auth_UserRoles");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Auth_Roles");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "Auth_RolePermissions");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "Auth_Permissions");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "Auth_UserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "Auth_UserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "Auth_UserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "Auth_RoleClaims");

            migrationBuilder.RenameIndex(
                name: "IX_Users_LastLoginAt",
                table: "Auth_Users",
                newName: "IX_Auth_Users_LastLoginAt");

            migrationBuilder.RenameIndex(
                name: "IX_Users_IsActive",
                table: "Auth_Users",
                newName: "IX_Auth_Users_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Auth_Users",
                newName: "IX_Auth_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Users_CreatedAt",
                table: "Auth_Users",
                newName: "IX_Auth_Users_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleId",
                table: "Auth_UserRoles",
                newName: "IX_Auth_UserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_IsActive",
                table: "Auth_UserRoles",
                newName: "IX_Auth_UserRoles_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_ExpiresAt",
                table: "Auth_UserRoles",
                newName: "IX_Auth_UserRoles_ExpiresAt");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_AssignedAt",
                table: "Auth_UserRoles",
                newName: "IX_Auth_UserRoles_AssignedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Priority",
                table: "Auth_Roles",
                newName: "IX_Auth_Roles_Priority");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_IsSystemRole",
                table: "Auth_Roles",
                newName: "IX_Auth_Roles_IsSystemRole");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_IsActive",
                table: "Auth_Roles",
                newName: "IX_Auth_Roles_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "Auth_RolePermissions",
                newName: "IX_Auth_RolePermissions_RoleId_PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "Auth_RolePermissions",
                newName: "IX_Auth_RolePermissions_PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_IsActive",
                table: "Auth_RolePermissions",
                newName: "IX_Auth_RolePermissions_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_GrantedAt",
                table: "Auth_RolePermissions",
                newName: "IX_Auth_RolePermissions_GrantedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_Name",
                table: "Auth_Permissions",
                newName: "IX_Auth_Permissions_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_Module",
                table: "Auth_Permissions",
                newName: "IX_Auth_Permissions_Module");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_IsSystemPermission",
                table: "Auth_Permissions",
                newName: "IX_Auth_Permissions_IsSystemPermission");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_IsActive",
                table: "Auth_Permissions",
                newName: "IX_Auth_Permissions_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_Category",
                table: "Auth_Permissions",
                newName: "IX_Auth_Permissions_Category");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "Auth_UserLogins",
                newName: "IX_Auth_UserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "Auth_UserClaims",
                newName: "IX_Auth_UserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "Auth_RoleClaims",
                newName: "IX_Auth_RoleClaims_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_Users",
                table: "Auth_Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_UserRoles",
                table: "Auth_UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_Roles",
                table: "Auth_Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_RolePermissions",
                table: "Auth_RolePermissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_Permissions",
                table: "Auth_Permissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_UserTokens",
                table: "Auth_UserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_UserLogins",
                table: "Auth_UserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_UserClaims",
                table: "Auth_UserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auth_RoleClaims",
                table: "Auth_RoleClaims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_RoleClaims_Auth_Roles_RoleId",
                table: "Auth_RoleClaims",
                column: "RoleId",
                principalTable: "Auth_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_RolePermissions_Auth_Permissions_PermissionId",
                table: "Auth_RolePermissions",
                column: "PermissionId",
                principalTable: "Auth_Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_RolePermissions_Auth_Roles_RoleId",
                table: "Auth_RolePermissions",
                column: "RoleId",
                principalTable: "Auth_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_UserClaims_Auth_Users_UserId",
                table: "Auth_UserClaims",
                column: "UserId",
                principalTable: "Auth_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_UserLogins_Auth_Users_UserId",
                table: "Auth_UserLogins",
                column: "UserId",
                principalTable: "Auth_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_UserRoles_Auth_Roles_RoleId",
                table: "Auth_UserRoles",
                column: "RoleId",
                principalTable: "Auth_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_UserRoles_Auth_Users_UserId",
                table: "Auth_UserRoles",
                column: "UserId",
                principalTable: "Auth_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_UserTokens_Auth_Users_UserId",
                table: "Auth_UserTokens",
                column: "UserId",
                principalTable: "Auth_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auth_RoleClaims_Auth_Roles_RoleId",
                table: "Auth_RoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_Auth_RolePermissions_Auth_Permissions_PermissionId",
                table: "Auth_RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Auth_RolePermissions_Auth_Roles_RoleId",
                table: "Auth_RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Auth_UserClaims_Auth_Users_UserId",
                table: "Auth_UserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_Auth_UserLogins_Auth_Users_UserId",
                table: "Auth_UserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_Auth_UserRoles_Auth_Roles_RoleId",
                table: "Auth_UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Auth_UserRoles_Auth_Users_UserId",
                table: "Auth_UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Auth_UserTokens_Auth_Users_UserId",
                table: "Auth_UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_UserTokens",
                table: "Auth_UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_Users",
                table: "Auth_Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_UserRoles",
                table: "Auth_UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_UserLogins",
                table: "Auth_UserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_UserClaims",
                table: "Auth_UserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_Roles",
                table: "Auth_Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_RolePermissions",
                table: "Auth_RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_RoleClaims",
                table: "Auth_RoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auth_Permissions",
                table: "Auth_Permissions");

            migrationBuilder.RenameTable(
                name: "Auth_UserTokens",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "Auth_Users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Auth_UserRoles",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "Auth_UserLogins",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "Auth_UserClaims",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "Auth_Roles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "Auth_RolePermissions",
                newName: "RolePermissions");

            migrationBuilder.RenameTable(
                name: "Auth_RoleClaims",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "Auth_Permissions",
                newName: "Permissions");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Users_LastLoginAt",
                table: "Users",
                newName: "IX_Users_LastLoginAt");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Users_IsActive",
                table: "Users",
                newName: "IX_Users_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Users_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Users_CreatedAt",
                table: "Users",
                newName: "IX_Users_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_UserRoles_RoleId",
                table: "UserRoles",
                newName: "IX_UserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_UserRoles_IsActive",
                table: "UserRoles",
                newName: "IX_UserRoles_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_UserRoles_ExpiresAt",
                table: "UserRoles",
                newName: "IX_UserRoles_ExpiresAt");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_UserRoles_AssignedAt",
                table: "UserRoles",
                newName: "IX_UserRoles_AssignedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_UserLogins_UserId",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_UserClaims_UserId",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Roles_Priority",
                table: "Roles",
                newName: "IX_Roles_Priority");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Roles_IsSystemRole",
                table: "Roles",
                newName: "IX_Roles_IsSystemRole");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Roles_IsActive",
                table: "Roles",
                newName: "IX_Roles_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                newName: "IX_RolePermissions_RoleId_PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_RolePermissions_PermissionId",
                table: "RolePermissions",
                newName: "IX_RolePermissions_PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_RolePermissions_IsActive",
                table: "RolePermissions",
                newName: "IX_RolePermissions_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_RolePermissions_GrantedAt",
                table: "RolePermissions",
                newName: "IX_RolePermissions_GrantedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_RoleClaims_RoleId",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Permissions_Name",
                table: "Permissions",
                newName: "IX_Permissions_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Permissions_Module",
                table: "Permissions",
                newName: "IX_Permissions_Module");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Permissions_IsSystemPermission",
                table: "Permissions",
                newName: "IX_Permissions_IsSystemPermission");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Permissions_IsActive",
                table: "Permissions",
                newName: "IX_Permissions_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Auth_Permissions_Category",
                table: "Permissions",
                newName: "IX_Permissions_Category");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_Roles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
