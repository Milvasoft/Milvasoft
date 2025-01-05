﻿using Microsoft.EntityFrameworkCore.Migrations;
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
            name: "AnotherFullAuditableEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: true),
                SomeDateProp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                SomeDateTimeOffsetProp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                table.PrimaryKey("PK_AnotherFullAuditableEntities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "BaseEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: true),
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
                table.PrimaryKey("PK_BaseEntities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Entities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: true),
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
                SomeStringProp = table.Column<string>(type: "text", nullable: true),
                SomeDateProp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                SomeDateTimeOffsetProp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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

        migrationBuilder.CreateTable(
            name: "HasTranslationEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Priority = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HasTranslationEntities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RestTestEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: true),
                Price = table.Column<decimal>(type: "numeric", nullable: false),
                Count = table.Column<int>(type: "integer", nullable: false),
                Number = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: true),
                InsertDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatorUserName = table.Column<string>(type: "text", nullable: true),
                LastModificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastModifierUserName = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RestTestEntities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SomeCircularReferenceEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: true),
                SomeDateProp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                SomeDecimalProp = table.Column<decimal>(type: "numeric", nullable: false),
                SomeEntityId = table.Column<int>(type: "integer", nullable: true),
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
                table.PrimaryKey("PK_SomeCircularReferenceEntities", x => x.Id);
                table.ForeignKey(
                    name: "FK_SomeCircularReferenceEntities_SomeCircularReferenceEntities~",
                    column: x => x.SomeEntityId,
                    principalTable: "SomeCircularReferenceEntities",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "TranslationEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: true),
                Description = table.Column<string>(type: "text", nullable: true),
                EntityId = table.Column<int>(type: "integer", nullable: false),
                LanguageId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TranslationEntities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RelatedEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: true),
                SomeDateProp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                SomeDecimalProp = table.Column<decimal>(type: "numeric", nullable: false),
                EntityId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RelatedEntities", x => x.Id);
                table.ForeignKey(
                    name: "FK_RelatedEntities_Entities_EntityId",
                    column: x => x.EntityId,
                    principalTable: "Entities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SomeManyToManyEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeEntityId = table.Column<int>(type: "integer", nullable: false),
                AnotherEntityId = table.Column<int>(type: "integer", nullable: false),
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
                table.PrimaryKey("PK_SomeManyToManyEntities", x => x.Id);
                table.ForeignKey(
                    name: "FK_SomeManyToManyEntities_AnotherFullAuditableEntities_Another~",
                    column: x => x.AnotherEntityId,
                    principalTable: "AnotherFullAuditableEntities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SomeManyToManyEntities_FullAuditableEntities_SomeEntityId",
                    column: x => x.SomeEntityId,
                    principalTable: "FullAuditableEntities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SomeManyToOneEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SomeStringProp = table.Column<string>(type: "text", nullable: true),
                SomeDateProp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                SomeDecimalProp = table.Column<decimal>(type: "numeric", nullable: false),
                SomeFullAuditableEntityId = table.Column<int>(type: "integer", nullable: false),
                SomeEntityId = table.Column<int>(type: "integer", nullable: false),
                AnotherFullAuditableEntityFixtureId = table.Column<int>(type: "integer", nullable: true),
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
                table.PrimaryKey("PK_SomeManyToOneEntities", x => x.Id);
                table.ForeignKey(
                    name: "FK_SomeManyToOneEntities_AnotherFullAuditableEntities_AnotherF~",
                    column: x => x.AnotherFullAuditableEntityFixtureId,
                    principalTable: "AnotherFullAuditableEntities",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_SomeManyToOneEntities_Entities_SomeEntityId",
                    column: x => x.SomeEntityId,
                    principalTable: "Entities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SomeManyToOneEntities_FullAuditableEntities_SomeFullAuditab~",
                    column: x => x.SomeFullAuditableEntityId,
                    principalTable: "FullAuditableEntities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RestChildrenTestEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: true),
                ParentId = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RestChildrenTestEntities", x => x.Id);
                table.ForeignKey(
                    name: "FK_RestChildrenTestEntities_RestTestEntities_ParentId",
                    column: x => x.ParentId,
                    principalTable: "RestTestEntities",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_RelatedEntities_EntityId",
            table: "RelatedEntities",
            column: "EntityId");

        migrationBuilder.CreateIndex(
            name: "IX_RestChildrenTestEntities_ParentId",
            table: "RestChildrenTestEntities",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_SomeCircularReferenceEntities_SomeEntityId",
            table: "SomeCircularReferenceEntities",
            column: "SomeEntityId");

        migrationBuilder.CreateIndex(
            name: "IX_SomeManyToManyEntities_AnotherEntityId",
            table: "SomeManyToManyEntities",
            column: "AnotherEntityId");

        migrationBuilder.CreateIndex(
            name: "IX_SomeManyToManyEntities_SomeEntityId",
            table: "SomeManyToManyEntities",
            column: "SomeEntityId");

        migrationBuilder.CreateIndex(
            name: "IX_SomeManyToOneEntities_AnotherFullAuditableEntityFixtureId",
            table: "SomeManyToOneEntities",
            column: "AnotherFullAuditableEntityFixtureId");

        migrationBuilder.CreateIndex(
            name: "IX_SomeManyToOneEntities_SomeEntityId",
            table: "SomeManyToOneEntities",
            column: "SomeEntityId");

        migrationBuilder.CreateIndex(
            name: "IX_SomeManyToOneEntities_SomeFullAuditableEntityId",
            table: "SomeManyToOneEntities",
            column: "SomeFullAuditableEntityId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BaseEntities");

        migrationBuilder.DropTable(
            name: "HasTranslationEntities");

        migrationBuilder.DropTable(
            name: "RelatedEntities");

        migrationBuilder.DropTable(
            name: "RestChildrenTestEntities");

        migrationBuilder.DropTable(
            name: "SomeCircularReferenceEntities");

        migrationBuilder.DropTable(
            name: "SomeManyToManyEntities");

        migrationBuilder.DropTable(
            name: "SomeManyToOneEntities");

        migrationBuilder.DropTable(
            name: "TranslationEntities");

        migrationBuilder.DropTable(
            name: "RestTestEntities");

        migrationBuilder.DropTable(
            name: "AnotherFullAuditableEntities");

        migrationBuilder.DropTable(
            name: "Entities");

        migrationBuilder.DropTable(
            name: "FullAuditableEntities");
    }
}
