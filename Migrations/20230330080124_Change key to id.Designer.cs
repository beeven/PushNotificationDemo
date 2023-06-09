﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using testpushnotification.Data;

#nullable disable

namespace testpushnotification.Migrations
{
    [DbContext(typeof(SubscriptionDbContext))]
    [Migration("20230330080124_Change key to id")]
    partial class Changekeytoid
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0-preview.2.23128.3");

            modelBuilder.Entity("testpushnotification.Data.ClientSubscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Auth")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("DateModified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Endpoint")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("Expires")
                        .HasColumnType("TEXT");

                    b.Property<string>("JwtToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("P256DH")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ClientId", "Endpoint");

                    b.ToTable("Subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
