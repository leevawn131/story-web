using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace story_web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    id_Category = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.id_Category);
                });

            migrationBuilder.CreateTable(
                name: "ChapterAudio",
                columns: table => new
                {
                    id_Audio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_Chapter = table.Column<int>(type: "int", nullable: true),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Generated_At = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterAudio", x => x.id_Audio);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id_User = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id_User);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    id_Author = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_User = table.Column<int>(type: "int", nullable: true),
                    PenName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.id_Author);
                    table.ForeignKey(
                        name: "FK_Authors_Users_id_User",
                        column: x => x.id_User,
                        principalTable: "Users",
                        principalColumn: "id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    id_Noti = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_User = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.id_Noti);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_id_User",
                        column: x => x.id_User,
                        principalTable: "Users",
                        principalColumn: "id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stories",
                columns: table => new
                {
                    id_Story = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_Author = table.Column<int>(type: "int", nullable: true),
                    StoryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Modified_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Views = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<double>(type: "float", nullable: true),
                    PostStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reject_Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Posted_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoryStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stories", x => x.id_Story);
                    table.ForeignKey(
                        name: "FK_Stories_Authors_id_Author",
                        column: x => x.id_Author,
                        principalTable: "Authors",
                        principalColumn: "id_Author",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    id_Chapter = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_Story = table.Column<int>(type: "int", nullable: true),
                    ChapterNumber = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ChapterName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Posted_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modified_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.id_Chapter);
                    table.ForeignKey(
                        name: "FK_Chapters_Stories_id_Story",
                        column: x => x.id_Story,
                        principalTable: "Stories",
                        principalColumn: "id_Story",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    id_Comment = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_Story = table.Column<int>(type: "int", nullable: true),
                    id_User = table.Column<int>(type: "int", nullable: true),
                    Posted_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.id_Comment);
                    table.ForeignKey(
                        name: "FK_Comments_Stories_id_Story",
                        column: x => x.id_Story,
                        principalTable: "Stories",
                        principalColumn: "id_Story");
                    table.ForeignKey(
                        name: "FK_Comments_Users_id_User",
                        column: x => x.id_User,
                        principalTable: "Users",
                        principalColumn: "id_User");
                });

            migrationBuilder.CreateTable(
                name: "Favourites",
                columns: table => new
                {
                    id_Favourite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_Story = table.Column<int>(type: "int", nullable: true),
                    id_User = table.Column<int>(type: "int", nullable: true),
                    Added_At = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favourites", x => x.id_Favourite);
                    table.ForeignKey(
                        name: "FK_Favourites_Stories_id_Story",
                        column: x => x.id_Story,
                        principalTable: "Stories",
                        principalColumn: "id_Story",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Favourites_Users_id_User",
                        column: x => x.id_User,
                        principalTable: "Users",
                        principalColumn: "id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoryCategories",
                columns: table => new
                {
                    id_Story = table.Column<int>(type: "int", nullable: false),
                    id_Category = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryCategories", x => new { x.id_Story, x.id_Category });
                    table.ForeignKey(
                        name: "FK_StoryCategories_Categories_id_Category",
                        column: x => x.id_Category,
                        principalTable: "Categories",
                        principalColumn: "id_Category",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoryCategories_Stories_id_Story",
                        column: x => x.id_Story,
                        principalTable: "Stories",
                        principalColumn: "id_Story",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reading_History",
                columns: table => new
                {
                    id_History = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_Story = table.Column<int>(type: "int", nullable: true),
                    id_User = table.Column<int>(type: "int", nullable: true),
                    id_Chapter = table.Column<int>(type: "int", nullable: true),
                    Last_Read_At = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reading_History", x => x.id_History);
                    table.ForeignKey(
                        name: "FK_Reading_History_Chapters_id_Chapter",
                        column: x => x.id_Chapter,
                        principalTable: "Chapters",
                        principalColumn: "id_Chapter",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reading_History_Stories_id_Story",
                        column: x => x.id_Story,
                        principalTable: "Stories",
                        principalColumn: "id_Story",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reading_History_Users_id_User",
                        column: x => x.id_User,
                        principalTable: "Users",
                        principalColumn: "id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authors_id_User",
                table: "Authors",
                column: "id_User",
                unique: true,
                filter: "[id_User] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_id_Story",
                table: "Chapters",
                column: "id_Story");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_id_Story",
                table: "Comments",
                column: "id_Story");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_id_User",
                table: "Comments",
                column: "id_User");

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_id_Story",
                table: "Favourites",
                column: "id_Story");

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_id_User",
                table: "Favourites",
                column: "id_User");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_id_User",
                table: "Notifications",
                column: "id_User");

            migrationBuilder.CreateIndex(
                name: "IX_Reading_History_id_Chapter",
                table: "Reading_History",
                column: "id_Chapter");

            migrationBuilder.CreateIndex(
                name: "IX_Reading_History_id_Story",
                table: "Reading_History",
                column: "id_Story");

            migrationBuilder.CreateIndex(
                name: "IX_Reading_History_id_User",
                table: "Reading_History",
                column: "id_User");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_id_Author",
                table: "Stories",
                column: "id_Author");

            migrationBuilder.CreateIndex(
                name: "IX_StoryCategories_id_Category",
                table: "StoryCategories",
                column: "id_Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterAudio");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Favourites");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reading_History");

            migrationBuilder.DropTable(
                name: "StoryCategories");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Stories");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
