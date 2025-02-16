﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuizAPI.Data;

#nullable disable

namespace QuizAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("QuizAPI.Models.Attempt", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("FeedbackId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("QuizId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("TotalScore")
                        .HasColumnType("float");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.HasIndex("QuizId");

                    b.HasIndex("UserId");

                    b.ToTable("Attempts");
                });

            modelBuilder.Entity("QuizAPI.Models.AttemptedQuestion", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Answers")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("AttemptId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("IsCorrect")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("QuestionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Score")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("AttemptId");

                    b.HasIndex("QuestionId");

                    b.ToTable("AttemptedQuestions");
                });

            modelBuilder.Entity("QuizAPI.Models.Feedback", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AttemptId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AttemptId")
                        .IsUnique();

                    b.ToTable("Feedbacks");
                });

            modelBuilder.Entity("QuizAPI.Models.Question", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Answers")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Options")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("QuizId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("QuizId");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("QuizAPI.Models.Quiz", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Duration")
                        .HasColumnType("int");

                    b.Property<bool>("IsLocked")
                        .HasColumnType("bit");

                    b.Property<Guid?>("QuizNodeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("QuizNodeId");

                    b.ToTable("Quizzes");
                });

            modelBuilder.Entity("QuizAPI.Models.QuizNode", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("QuizNodes");
                });

            modelBuilder.Entity("QuizAPI.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("QuizAPI.Models.Attempt", b =>
                {
                    b.HasOne("QuizAPI.Models.User", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("QuizAPI.Models.Quiz", "Quiz")
                        .WithMany()
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuizAPI.Models.User", null)
                        .WithMany("Attempts")
                        .HasForeignKey("UserId");

                    b.Navigation("Player");

                    b.Navigation("Quiz");
                });

            modelBuilder.Entity("QuizAPI.Models.AttemptedQuestion", b =>
                {
                    b.HasOne("QuizAPI.Models.Attempt", null)
                        .WithMany("Questions")
                        .HasForeignKey("AttemptId");

                    b.HasOne("QuizAPI.Models.Question", "Question")
                        .WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");
                });

            modelBuilder.Entity("QuizAPI.Models.Feedback", b =>
                {
                    b.HasOne("QuizAPI.Models.Attempt", "Attempt")
                        .WithOne("Feedback")
                        .HasForeignKey("QuizAPI.Models.Feedback", "AttemptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Attempt");
                });

            modelBuilder.Entity("QuizAPI.Models.Question", b =>
                {
                    b.HasOne("QuizAPI.Models.User", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("QuizAPI.Models.Quiz", "Quiz")
                        .WithMany("Questions")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("Quiz");
                });

            modelBuilder.Entity("QuizAPI.Models.Quiz", b =>
                {
                    b.HasOne("QuizAPI.Models.User", "Author")
                        .WithMany("Quizzes")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuizAPI.Models.QuizNode", null)
                        .WithMany("Quizzes")
                        .HasForeignKey("QuizNodeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Author");
                });

            modelBuilder.Entity("QuizAPI.Models.QuizNode", b =>
                {
                    b.OwnsOne("QuizAPI.Models.Location", "Location", b1 =>
                        {
                            b1.Property<Guid>("QuizNodeId")
                                .HasColumnType("uniqueidentifier");

                            b1.PrimitiveCollection<string>("Coordinates")
                                .IsRequired()
                                .HasColumnType("nvarchar(450)");

                            b1.Property<string>("Type")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("QuizNodeId");

                            b1.HasIndex("Coordinates");

                            b1.ToTable("QuizNodes");

                            b1.WithOwner()
                                .HasForeignKey("QuizNodeId");
                        });

                    b.Navigation("Location")
                        .IsRequired();
                });

            modelBuilder.Entity("QuizAPI.Models.Attempt", b =>
                {
                    b.Navigation("Feedback")
                        .IsRequired();

                    b.Navigation("Questions");
                });

            modelBuilder.Entity("QuizAPI.Models.Quiz", b =>
                {
                    b.Navigation("Questions");
                });

            modelBuilder.Entity("QuizAPI.Models.QuizNode", b =>
                {
                    b.Navigation("Quizzes");
                });

            modelBuilder.Entity("QuizAPI.Models.User", b =>
                {
                    b.Navigation("Attempts");

                    b.Navigation("Quizzes");
                });
#pragma warning restore 612, 618
        }
    }
}
