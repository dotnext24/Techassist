using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechAssistPro.Scheduling.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchedulingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupportAgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReassignmentCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportAgents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveAssignments = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportAgents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportAgentSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    SupportAgentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportAgentSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportAgentSkills_SupportAgents_SupportAgentId",
                        column: x => x.SupportAgentId,
                        principalTable: "SupportAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SupportAgents",
                columns: new[] { "Id", "IsAvailable", "ActiveAssignments", "CreatedAtUtc", "IsDeleted", "LastUpdatedAtUtc", "Name", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), true, 0, new DateTime(2025, 12, 21, 10, 33, 45, 778, DateTimeKind.Utc).AddTicks(1944), false, null, "Arjun", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), true, 0, new DateTime(2025, 12, 21, 10, 33, 45, 778, DateTimeKind.Utc).AddTicks(1947), false, null, "Priya", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), true, 0, new DateTime(2025, 12, 21, 10, 33, 45, 778, DateTimeKind.Utc).AddTicks(1948), false, null, "Rohit", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), true, 0, new DateTime(2025, 12, 21, 10, 33, 45, 778, DateTimeKind.Utc).AddTicks(1949), false, null, "Sneha", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), true, 0, new DateTime(2025, 12, 21, 10, 33, 45, 778, DateTimeKind.Utc).AddTicks(1949), false, null, "Karthik", null }
                });

            migrationBuilder.InsertData(
                table: "SupportAgentSkills",
                columns: new[] { "Id", "Category", "Level", "SupportAgentId" },
                values: new object[,]
                {
                    { new Guid("12dd110b-38fc-47d4-9ff0-ad5cda65fee1"), "Network", 3, new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("2c5084bc-1f3a-42ad-9104-7b45ba83232e"), "Network", 2, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("896bf62d-0cab-475e-90cb-abbafe6ae1ee"), "Hardware", 3, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a5b043d5-0c45-43e9-bfe8-184fbea25cce"), "Hardware", 2, new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("b43d4c07-c805-4d56-9fbd-d1401d0345d5"), "Software", 3, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("b8dd0cd4-0c91-4788-a7c8-7d1b826e1a3f"), "Software", 2, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("b95d270a-b7f5-49f7-8c08-4bc62737b7d8"), "Database", 2, new Guid("33333333-3333-3333-3333-333333333333") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TicketId",
                table: "Assignments",
                column: "TicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportAgentSkills_SupportAgentId",
                table: "SupportAgentSkills",
                column: "SupportAgentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "SupportAgentSkills");

            migrationBuilder.DropTable(
                name: "SupportAgents");
        }
    }
}
