using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMan.Migrations
{
    /// <inheritdoc />
    public partial class Model_Relationships_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupListTeam");

            migrationBuilder.AlterColumn<string>(
                name: "User_ID",
                table: "UserTasks",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_User_ID",
                table: "UserTasks",
                column: "User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GroupUsers_Group_ID",
                table: "GroupUsers",
                column: "Group_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GroupList_Group_ID",
                table: "GroupList",
                column: "Group_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupList_Group_Group_ID",
                table: "GroupList",
                column: "Group_ID",
                principalTable: "Group",
                principalColumn: "Group_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupUsers_AspNetUsers_User_ID",
                table: "GroupUsers",
                column: "User_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupUsers_Group_Group_ID",
                table: "GroupUsers",
                column: "Group_ID",
                principalTable: "Group",
                principalColumn: "Group_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTasks_AspNetUsers_User_ID",
                table: "UserTasks",
                column: "User_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupList_Group_Group_ID",
                table: "GroupList");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupUsers_AspNetUsers_User_ID",
                table: "GroupUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupUsers_Group_Group_ID",
                table: "GroupUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTasks_AspNetUsers_User_ID",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_UserTasks_User_ID",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_GroupUsers_Group_ID",
                table: "GroupUsers");

            migrationBuilder.DropIndex(
                name: "IX_GroupList_Group_ID",
                table: "GroupList");

            migrationBuilder.AlterColumn<string>(
                name: "User_ID",
                table: "UserTasks",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupListTeam",
                columns: table => new
                {
                    User_ID = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Group_List_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupListTeam", x => new { x.User_ID, x.Group_List_ID });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
