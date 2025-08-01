﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Milvasoft.IntegrationTests.Client.Migrations
{
    [DbContext(typeof(MilvaBulkDbContextFixture))]
    [Migration("20250731220800_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.AnotherFullAuditableEntityFixture", b =>
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

                    b.Property<int?>("FullAuditableEntityId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModificationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastModifierUserName")
                        .HasColumnType("text");

                    b.Property<DateTime>("SomeDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("SomeDateTimeOffsetProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FullAuditableEntityId")
                        .IsUnique();

                    b.ToTable("AnotherFullAuditableEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.HasJsonTranslationEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.Property<List<JsonTranslationEntityFixture>>("Translations")
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.ToTable("HasJsonTranslationEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.HasTranslationEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("HasTranslationEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.RestChildrenTestEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("RestChildrenTestEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.RestTestEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<DateTime>("InsertDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModificationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastModifierUserName")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("RestTestEntities");
                });

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

                    b.Property<int?>("SomeNullableProp")
                        .HasColumnType("integer");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("BaseEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeCircularReferenceFullAuditableEntityFixture", b =>
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

                    b.Property<int?>("SomeEntityId")
                        .HasColumnType("integer");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SomeEntityId");

                    b.ToTable("SomeCircularReferenceEntities");
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

                    b.Property<DateTime>("SomeDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("SomeDateTimeOffsetProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("FullAuditableEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeLogEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CacheInfo")
                        .HasColumnType("text");

                    b.Property<string>("ClassName")
                        .HasColumnType("text");

                    b.Property<int>("ElapsedMs")
                        .HasColumnType("integer");

                    b.Property<string>("Exception")
                        .HasColumnType("text");

                    b.Property<bool>("IsSuccess")
                        .HasColumnType("boolean");

                    b.Property<string>("MethodName")
                        .HasColumnType("text");

                    b.Property<string>("MethodParams")
                        .HasColumnType("text");

                    b.Property<string>("MethodResult")
                        .HasColumnType("text");

                    b.Property<string>("Namespace")
                        .HasColumnType("text");

                    b.Property<string>("TransactionId")
                        .HasColumnType("text");

                    b.Property<DateTime>("UtcLogTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("SomeLogEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeManyToManyEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AnotherEntityId")
                        .HasColumnType("integer");

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

                    b.Property<int>("SomeEntityId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AnotherEntityId");

                    b.HasIndex("SomeEntityId");

                    b.ToTable("SomeManyToManyEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeManyToOneFullAuditableEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("AnotherFullAuditableEntityFixtureId")
                        .HasColumnType("integer");

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

                    b.Property<int>("SomeEntityId")
                        .HasColumnType("integer");

                    b.Property<int>("SomeFullAuditableEntityId")
                        .HasColumnType("integer");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AnotherFullAuditableEntityFixtureId");

                    b.HasIndex("SomeEntityId");

                    b.HasIndex("SomeFullAuditableEntityId");

                    b.ToTable("SomeManyToOneEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeModelBuilderTestEntityFixture", b =>
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

                    b.Property<DateTimeOffset>("SomeDateTimeOffsetProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal?>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeEncryptedStringProp")
                        .HasColumnType("text");

                    b.Property<string>("SomeEncryptedStringWithAttributeProp")
                        .HasColumnType("text");

                    b.Property<int>("SomeIntProp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1);

                    b.Property<DateTime?>("SomeNullableDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("SomeNullableDateTimeOffsetProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TenantId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ModelBuilderTestEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeModelBuilderTestKeylessEntityFixture", b =>
                {
                    b.Property<DateTime>("SomeDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("SomeDateTimeOffsetProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal?>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeEncryptedStringProp")
                        .HasColumnType("text");

                    b.Property<string>("SomeEncryptedStringWithAttributeProp")
                        .HasColumnType("text");

                    b.Property<int>("SomeIntProp")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("SomeNullableDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("SomeNullableDateTimeOffsetProp")
                        .HasColumnType("timestamp with time zone");

                    b.ToTable("ModelBuilderTestKeylessEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeMultiTenantTestEntityFixture", b =>
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

                    b.Property<DateTimeOffset>("SomeDateTimeOffsetProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal?>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeEncryptedStringProp")
                        .HasColumnType("text");

                    b.Property<string>("SomeEncryptedStringWithAttributeProp")
                        .HasColumnType("text");

                    b.Property<int>("SomeIntProp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1);

                    b.Property<DateTime?>("SomeNullableDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("SomeNullableDateTimeOffsetProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("SomeMultiTenantEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeRelatedEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("EntityId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("SomeDateProp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("SomeDecimalProp")
                        .HasColumnType("numeric");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.ToTable("RelatedEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeTenantEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<string>("ConnectionString")
                        .HasColumnType("text");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<string>("DeleterUserName")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DeletionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModificationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastModifierUserName")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("SomeStringProp")
                        .HasColumnType("text");

                    b.Property<DateTime>("SubscriptionExpireDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TenancyName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("SomeTenantEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.TranslationEntityFixture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int>("EntityId")
                        .HasColumnType("integer");

                    b.Property<int>("LanguageId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("TranslationEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.AnotherFullAuditableEntityFixture", b =>
                {
                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeFullAuditableEntityFixture", "FullAuditableEntity")
                        .WithOne("RelatedFullAuditableEntity")
                        .HasForeignKey("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.AnotherFullAuditableEntityFixture", "FullAuditableEntityId");

                    b.Navigation("FullAuditableEntity");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.RestChildrenTestEntityFixture", b =>
                {
                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.RestTestEntityFixture", "Parent")
                        .WithMany("Childrens")
                        .HasForeignKey("ParentId");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeCircularReferenceFullAuditableEntityFixture", b =>
                {
                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeCircularReferenceFullAuditableEntityFixture", "SomeEntity")
                        .WithMany()
                        .HasForeignKey("SomeEntityId");

                    b.Navigation("SomeEntity");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeManyToManyEntityFixture", b =>
                {
                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.AnotherFullAuditableEntityFixture", "AnotherEntity")
                        .WithMany()
                        .HasForeignKey("AnotherEntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeFullAuditableEntityFixture", "SomeEntity")
                        .WithMany()
                        .HasForeignKey("SomeEntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AnotherEntity");

                    b.Navigation("SomeEntity");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeManyToOneFullAuditableEntityFixture", b =>
                {
                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.AnotherFullAuditableEntityFixture", null)
                        .WithMany("ManyToOneEntities")
                        .HasForeignKey("AnotherFullAuditableEntityFixtureId");

                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeEntityFixture", "SomeEntity")
                        .WithMany("ManyToOneEntities")
                        .HasForeignKey("SomeEntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeFullAuditableEntityFixture", "SomeFullAuditableEntity")
                        .WithMany("ManyToOneEntities")
                        .HasForeignKey("SomeFullAuditableEntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SomeEntity");

                    b.Navigation("SomeFullAuditableEntity");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeRelatedEntityFixture", b =>
                {
                    b.HasOne("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeEntityFixture", "Entity")
                        .WithMany("RelatedEntities")
                        .HasForeignKey("EntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.AnotherFullAuditableEntityFixture", b =>
                {
                    b.Navigation("ManyToOneEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.RestTestEntityFixture", b =>
                {
                    b.Navigation("Childrens");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeEntityFixture", b =>
                {
                    b.Navigation("ManyToOneEntities");

                    b.Navigation("RelatedEntities");
                });

            modelBuilder.Entity("Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures.SomeFullAuditableEntityFixture", b =>
                {
                    b.Navigation("ManyToOneEntities");

                    b.Navigation("RelatedFullAuditableEntity");
                });
#pragma warning restore 612, 618
        }
    }
}
