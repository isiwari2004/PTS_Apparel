using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTS_Apparel.Migrations
{
    /// <inheritdoc />
    public partial class AddFactoryMasterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoryMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactoryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StyleCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LineNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForecastDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ForecastQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecasts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InputRecorders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LineNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PONo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StyleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Colour = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Total = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputRecorders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "POs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PONumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Style = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Tolerance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SizeBreakdownJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreenPrivileges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanAdd = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenPrivileges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Styles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StyleCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sizes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Styles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactoryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactoryMasterId = table.Column<int>(type: "int", nullable: false),
                    WorkingHours = table.Column<int>(type: "int", nullable: false),
                    CycleTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProdLines = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoryDetails_FactoryMasters_FactoryMasterId",
                        column: x => x.FactoryMasterId,
                        principalTable: "FactoryMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FactoryDetails_FactoryMasterId",
                table: "FactoryDetails",
                column: "FactoryMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "FactoryDetails");

            migrationBuilder.DropTable(
                name: "Forecasts");

            migrationBuilder.DropTable(
                name: "InputRecorders");

            migrationBuilder.DropTable(
                name: "POs");

            migrationBuilder.DropTable(
                name: "ScreenPrivileges");

            migrationBuilder.DropTable(
                name: "Styles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "FactoryMasters");
        }
    }
}
