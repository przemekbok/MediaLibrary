﻿// <auto-generated />
using System;
using MediaLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MediaLibrary.Migrations
{
    [DbContext(typeof(MediaLibraryContext))]
    [Migration("20250313004421_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Media", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Duration")
                        .HasColumnType("int");

                    b.Property<string>("FileExtension")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("FileType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("Height")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("ThumbnailPath")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("UploadDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Width")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FileType");

                    b.HasIndex("Name");

                    b.HasIndex("UploadDate");

                    b.ToTable("Media");
                });

            modelBuilder.Entity("MediaLibrary.Models.MediaTag", b =>
                {
                    b.Property<int>("MediaId")
                        .HasColumnType("int");

                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.HasKey("MediaId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("MediaTags");
                });

            modelBuilder.Entity("MediaLibrary.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("MediaLibrary.Models.MediaTag", b =>
                {
                    b.HasOne("Media", "Media")
                        .WithMany("MediaTags")
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MediaLibrary.Models.Tag", "Tag")
                        .WithMany("MediaTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Media");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Media", b =>
                {
                    b.Navigation("MediaTags");
                });

            modelBuilder.Entity("MediaLibrary.Models.Tag", b =>
                {
                    b.Navigation("MediaTags");
                });
#pragma warning restore 612, 618
        }
    }
}
