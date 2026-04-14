using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eVisaPlatform.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredVisaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicantFullName",
                table: "VisaApplications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationCountry",
                table: "VisaApplications",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IntendedTravelDate",
                table: "VisaApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "VisaApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "VisaApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantFullName",
                table: "VisaApplications");

            migrationBuilder.DropColumn(
                name: "DestinationCountry",
                table: "VisaApplications");

            migrationBuilder.DropColumn(
                name: "IntendedTravelDate",
                table: "VisaApplications");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "VisaApplications");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "VisaApplications");
        }
    }
}
