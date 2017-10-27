﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using TOIFeedServer;
using TOIFeedServer.Models;

namespace TOIFeedServer.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("TOIFeedServer.Models.ContextModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(70);

                    b.HasKey("Id");

                    b.ToTable("Contexts");
                });

            modelBuilder.Entity("TOIFeedServer.Models.PositionModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("TagModelId");

                    b.Property<double>("X");

                    b.Property<double>("Y");

                    b.HasKey("Id");

                    b.HasIndex("TagModelId");

                    b.ToTable("Positions");
                });

            modelBuilder.Entity("TOIFeedServer.Models.TagInfoModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Image");

                    b.Property<string>("Title");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("TagInfos");
                });

            modelBuilder.Entity("TOIFeedServer.Models.TagModel", b =>
                {
                    b.Property<Guid>("TagId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("TagType");

                    b.HasKey("TagId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("TOIFeedServer.Models.ToiModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ContextModelId");

                    b.Property<Guid?>("TagInfoModelId");

                    b.Property<Guid?>("TagModelTagId");

                    b.HasKey("Id");

                    b.HasIndex("ContextModelId");

                    b.HasIndex("TagInfoModelId");

                    b.HasIndex("TagModelTagId");

                    b.ToTable("Tois");
                });

            modelBuilder.Entity("TOIFeedServer.Models.PositionModel", b =>
                {
                    b.HasOne("TOIFeedServer.Models.TagModel", "TagModel")
                        .WithMany()
                        .HasForeignKey("TagModelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TOIFeedServer.Models.ToiModel", b =>
                {
                    b.HasOne("TOIFeedServer.Models.ContextModel", "ContextModel")
                        .WithMany()
                        .HasForeignKey("ContextModelId");

                    b.HasOne("TOIFeedServer.Models.TagInfoModel", "TagInfoModel")
                        .WithMany()
                        .HasForeignKey("TagInfoModelId");

                    b.HasOne("TOIFeedServer.Models.TagModel", "TagModel")
                        .WithMany()
                        .HasForeignKey("TagModelTagId");
                });
#pragma warning restore 612, 618
        }
    }
}
