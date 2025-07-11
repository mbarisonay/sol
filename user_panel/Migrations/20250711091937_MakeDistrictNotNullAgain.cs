using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace user_panel.Migrations
{
    /// <inheritdoc />
    public partial class MakeDistrictNotNullAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "cab",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "cab",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "cab",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "cab",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "cab",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "cab",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "Location",
                table: "cab");

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "cab",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "city",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_city", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "district",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_district", x => x.Id);
                    table.ForeignKey(
                        name: "FK_district_city_CityId",
                        column: x => x.CityId,
                        principalTable: "city",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cab_DistrictId",
                table: "cab",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_district_CityId",
                table: "district",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_cab_district_DistrictId",
                table: "cab",
                column: "DistrictId",
                principalTable: "district",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cab_district_DistrictId",
                table: "cab");

            migrationBuilder.DropTable(
                name: "district");

            migrationBuilder.DropTable(
                name: "city");

            migrationBuilder.DropIndex(
                name: "IX_cab_DistrictId",
                table: "cab");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "cab");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "cab",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "cab",
                columns: new[] { "Id", "Description", "Location", "PricePerHour" },
                values: new object[,]
                {
                    { 1, "A modern, compact cabin perfect for a high-intensity workout. Equipped with a smart treadmill and weight set.", "Bornova/İzmir", 25.00m },
                    { 2, "Spacious cabin with a focus on yoga and flexibility. Includes a full-length mirror and yoga mats.", "Karşıyaka/İzmir", 25.00m },
                    { 3, "Premium cabin featuring a rowing machine and advanced monitoring systems for performance tracking.", "Çankaya/Ankara", 30.00m },
                    { 4, "Standard cabin with essential cardio and strength training equipment. Great for a balanced workout.", "Akyurt/Ankara", 20.00m },
                    { 5, "An urban-style cabin with a boxing bag and a high-performance stationary bike.", "Beşiktaş/İstanbul", 35.00m },
                    { 6, "Historic district cabin offering a quiet and serene environment for mindful exercise and meditation.", "Fatih/İstanbul", 30.00m }
                });
        }
    }
}
