using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecurringJob.API.Migrations
{
    /// <inheritdoc />
    public partial class init_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeTriggers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TimeExpression = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TriggerType = table.Column<int>(type: "int", nullable: false),
                    MethodName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NextExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTriggers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TriggerExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TriggerId = table.Column<int>(type: "int", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionDurationMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggerExecutionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriggerExecutionLogs_TimeTriggers_TriggerId",
                        column: x => x.TriggerId,
                        principalTable: "TimeTriggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTriggers_IsActive",
                table: "TimeTriggers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTriggers_NextExecutionTime",
                table: "TimeTriggers",
                column: "NextExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_TriggerExecutionLogs_ExecutedAt",
                table: "TriggerExecutionLogs",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TriggerExecutionLogs_TriggerId",
                table: "TriggerExecutionLogs",
                column: "TriggerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TriggerExecutionLogs");

            migrationBuilder.DropTable(
                name: "TimeTriggers");
        }
    }
}
