using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseApp.Migrations
{
    public partial class AddLoanDocumentPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "Loans",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "Loans");
        }
    }
}
