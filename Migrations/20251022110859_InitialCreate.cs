using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StringAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyzedStrings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    IsPalindrome = table.Column<bool>(type: "bit", nullable: false),
                    UniqueCharacters = table.Column<int>(type: "int", nullable: false),
                    WordCount = table.Column<int>(type: "int", nullable: false),
                    Sha256Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CharacterFrequencyMapJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyzedStrings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyzedStrings_Value",
                table: "AnalyzedStrings",
                column: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyzedStrings");
        }
    }
}
