﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Milvasoft.IntegrationTests.Client.Migrations
{
    [DbContext(typeof(MilvaBulkDbContextFixture))]
    partial class MilvaBulkDbContextFixtureModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeBaseEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<string>("DeleterUserName")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DeletionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModificationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastModifierUserName")
                        .HasColumnType("text");

                    b.Property<DateTime>("SomeDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("BaseEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("SomeDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Entities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeFullAuditableEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<string>("DeleterUserName")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DeletionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModificationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastModifierUserName")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("SomeDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("FullAuditableEntities");
                });
#pragma warning restore 612, 618
        }
    }
}
