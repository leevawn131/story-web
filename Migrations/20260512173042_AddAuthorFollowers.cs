using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace story_web.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorFollowers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterAudio");

            migrationBuilder.AddColumn<string>(
                name: "AISummary",
                table: "Chapters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioPath",
                table: "Chapters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthorFollowers",
                columns: table => new
                {
                    id_AuthorFollower = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_Author = table.Column<int>(type: "int", nullable: true),
                    id_User = table.Column<int>(type: "int", nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorFollowers", x => x.id_AuthorFollower);
                    table.ForeignKey(
                        name: "FK_AuthorFollowers_Authors_id_Author",
                        column: x => x.id_Author,
                        principalTable: "Authors",
                        principalColumn: "id_Author",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuthorFollowers_Users_id_User",
                        column: x => x.id_User,
                        principalTable: "Users",
                        principalColumn: "id_User");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorFollowers_id_Author",
                table: "AuthorFollowers",
                column: "id_Author");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorFollowers_id_User",
                table: "AuthorFollowers",
                column: "id_User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorFollowers");

            migrationBuilder.DropColumn(
                name: "AISummary",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "AudioPath",
                table: "Chapters");

            migrationBuilder.CreateTable(
                name: "ChapterAudio",
                columns: table => new
                {
                    id_Audio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Generated_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    id_Chapter = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterAudio", x => x.id_Audio);
                });
        }
    }
}
