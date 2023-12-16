﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Trsys.Web.Models;

#nullable disable

namespace Trsys.Web.Migrations
{
    [DbContext(typeof(TrsysContext))]
    [Migration("20231106144549_UpdateTypes")]
    partial class UpdateTypes
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Xml")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("Trsys.Models.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Exception")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Level")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MessageTemplate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Properties")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("TimeStamp")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("TimeStamp");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("Trsys.Models.Message", b =>
                {
                    b.Property<long>("Position")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Position"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime");

                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("JsonData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("JsonMetadata")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StreamIdInternal")
                        .HasColumnType("int");

                    b.Property<int>("StreamVersion")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Position")
                        .HasName("PK_Events");

                    b.HasIndex(new[] { "StreamIdInternal", "Created" }, "IX_Messages_StreamIdInternal_Created");

                    b.HasIndex(new[] { "StreamIdInternal", "Id" }, "IX_Messages_StreamIdInternal_Id")
                        .IsUnique();

                    b.HasIndex(new[] { "StreamIdInternal", "StreamVersion" }, "IX_Messages_StreamIdInternal_Revision")
                        .IsUnique();

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Trsys.Models.Stream", b =>
                {
                    b.Property<int>("IdInternal")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdInternal"));

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasMaxLength(42)
                        .IsUnicode(false)
                        .HasColumnType("char(42)")
                        .IsFixedLength();

                    b.Property<string>("IdOriginal")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("IdOriginalReversed")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)")
                        .HasComputedColumnSql("(reverse([IdOriginal]))", false);

                    b.Property<int?>("MaxAge")
                        .HasColumnType("int");

                    b.Property<int?>("MaxCount")
                        .HasColumnType("int");

                    b.Property<long>("Position")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValueSql("((-1))");

                    b.Property<int>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql("((-1))");

                    b.HasKey("IdInternal");

                    b.HasIndex(new[] { "Id" }, "IX_Streams_Id")
                        .IsUnique();

                    b.HasIndex(new[] { "IdOriginal", "IdInternal" }, "IX_Streams_IdOriginal");

                    b.HasIndex(new[] { "IdOriginalReversed", "IdInternal" }, "IX_Streams_IdOriginalReversed");

                    b.ToTable("Streams");
                });

            modelBuilder.Entity("Trsys.Models.Message", b =>
                {
                    b.HasOne("Trsys.Models.Stream", "StreamIdInternalNavigation")
                        .WithMany("Messages")
                        .HasForeignKey("StreamIdInternal")
                        .IsRequired()
                        .HasConstraintName("FK_Events_Streams");

                    b.Navigation("StreamIdInternalNavigation");
                });

            modelBuilder.Entity("Trsys.Models.Stream", b =>
                {
                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
