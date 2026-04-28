using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMan.Migrations
{
    /// <inheritdoc />
    public partial class AddTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TRIGGER AddInitialLists
                AFTER INSERT ON taskman.AspNetUsers
                FOR EACH ROW
                BEGIN
                    INSERT INTO taskman.usertasks (User_ID, Title, Description, Progress, BckGr_Col)
                    VALUES (new.Id, 'To-do', 'Tasks left to complete', 100, '#ababab');
                END;
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS AddInitialLists");
        }
    }
}
