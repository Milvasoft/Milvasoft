using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Milvasoft.IntegrationTests.Client.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BaseEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: false),
                SomeDateProp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                SomeDecimalProp = table.Column<decimal>(type: "numeric", nullable: false),
                CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatorUserName = table.Column<string>(type: "text", nullable: false),
                LastModificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastModifierUserName = table.Column<string>(type: "text", nullable: false),
                DeletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                DeleterUserName = table.Column<string>(type: "text", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BaseEntities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Entities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: false),
                SomeDateProp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                SomeDecimalProp = table.Column<decimal>(type: "numeric", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Entities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "FullAuditableEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: false),
                SomeDateProp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                SomeDecimalProp = table.Column<decimal>(type: "numeric", nullable: false),
                CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatorUserName = table.Column<string>(type: "text", nullable: true),
                LastModificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastModifierUserName = table.Column<string>(type: "text", nullable: true),
                DeletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                DeleterUserName = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FullAuditableEntities", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BaseEntities");

        migrationBuilder.DropTable(
            name: "Entities");

        migrationBuilder.DropTable(
            name: "FullAuditableEntities");
    }
}
