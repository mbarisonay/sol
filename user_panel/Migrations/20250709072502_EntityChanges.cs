using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace user_panel.Migrations
{
    /// <inheritdoc />
    public partial class EntityChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_ApplicationUserId1",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_cab_CabinId1",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CabinReservation_AspNetUsers_ApplicationUserId1",
                table: "CabinReservation");

            migrationBuilder.DropIndex(
                name: "IX_CabinReservation_ApplicationUserId1",
                table: "CabinReservation");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ApplicationUserId1",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CabinId1",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "CabinReservation");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CabinId1",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "CabinReservation",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CabinId1",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CabinReservation_ApplicationUserId1",
                table: "CabinReservation",
                column: "ApplicationUserId1",
                unique: true,
                filter: "[ApplicationUserId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ApplicationUserId1",
                table: "Bookings",
                column: "ApplicationUserId1",
                unique: true,
                filter: "[ApplicationUserId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CabinId1",
                table: "Bookings",
                column: "CabinId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_ApplicationUserId1",
                table: "Bookings",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_cab_CabinId1",
                table: "Bookings",
                column: "CabinId1",
                principalTable: "cab",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CabinReservation_AspNetUsers_ApplicationUserId1",
                table: "CabinReservation",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
