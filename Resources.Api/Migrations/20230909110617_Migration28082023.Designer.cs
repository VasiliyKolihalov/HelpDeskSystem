﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Resources.Api.Repositories;

#nullable disable

namespace Resources.Api.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20230909110617_Migration28082023")]
    partial class Migration28082023
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Resources.Api.Models.Images.Image", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Resources.Api.Models.Images.ImageMessage", b =>
                {
                    b.Property<Guid>("MessageId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ImageId")
                        .HasColumnType("uuid");

                    b.HasKey("MessageId", "ImageId");

                    b.HasIndex("ImageId");

                    b.ToTable("ImagesMessages");
                });

            modelBuilder.Entity("Resources.Api.Models.Images.ImageMessage", b =>
                {
                    b.HasOne("Resources.Api.Models.Images.Image", "Image")
                        .WithMany()
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");
                });
#pragma warning restore 612, 618
        }
    }
}
