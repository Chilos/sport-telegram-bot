using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sport_telegram_bot.Persistence.Migrations
{
    public partial class updateDateOnlyFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateAt",
                table: "TrainRecord",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAt",
                table: "TrainRecord",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
