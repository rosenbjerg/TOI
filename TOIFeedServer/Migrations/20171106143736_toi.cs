using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TOIFeedServer.Migrations
{
    public partial class toi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "BLOB", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contexts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "BLOB", nullable: false),
                    ContextModelId = table.Column<Guid>(type: "BLOB", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Image = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tois", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tois_Contexts_ContextModelId",
                        column: x => x.ContextModelId,
                        principalTable: "Contexts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagId = table.Column<Guid>(type: "BLOB", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longtitude = table.Column<double>(type: "REAL", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Radius = table.Column<int>(type: "INTEGER", nullable: false),
                    TagType = table.Column<int>(type: "INTEGER", nullable: false),
                    ToiModelId = table.Column<Guid>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagId);
                    table.ForeignKey(
                        name: "FK_Tags_Tois_ToiModelId",
                        column: x => x.ToiModelId,
                        principalTable: "Tois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ToiModelId",
                table: "Tags",
                column: "ToiModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Tois_ContextModelId",
                table: "Tois",
                column: "ContextModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Tois");

            migrationBuilder.DropTable(
                name: "Contexts");
        }
    }
}
