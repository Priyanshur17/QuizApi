//using Microsoft.EntityFrameworkCore;
//using QuizAPI.Models;

//namespace QuizAPI.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

//        public DbSet<User> Users { get; set; }
//        public DbSet<Quiz> Quizzes { get; set; }
//        public DbSet<Question> Questions { get; set; }
//        public DbSet<Attempt> Attempts { get; set; }
//        public DbSet<Feedback> Feedbacks { get; set; }
//        public DbSet<QuizNode> QuizNodes { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            // Geospatial index for QuizNode
//            modelBuilder.Entity<QuizNode>()
//                .OwnsOne(q => q.Location)
//                .HasIndex(l => l.Coordinates)
//                .IsUnique(false);

//            modelBuilder.Entity<Quiz>()
//            .HasMany(q => q.Questions)
//            .WithOne(q => q.Quiz)
//            .HasForeignKey(q => q.QuizId);
//            // Define relationships explicitly
//            modelBuilder.Entity<Question>()
//                .HasOne(q => q.Quiz)
//                .WithMany(q => q.Questions)
//                .HasForeignKey(q => q.QuizId)
//                .OnDelete(DeleteBehavior.Cascade); // Quiz deletion cascades to Questions

//            modelBuilder.Entity<Question>()
//                .HasOne(q => q.Author)
//                .WithMany() // No inverse navigation property required
//                .HasForeignKey(q => q.AuthorId)
//                .OnDelete(DeleteBehavior.Restrict); // Prevent cycle

//            modelBuilder.Entity<Attempt>()
//                .HasOne(a => a.Player)
//                .WithMany()
//                .HasForeignKey(a => a.PlayerId)
//                .OnDelete(DeleteBehavior.Restrict);  // Avoid multiple cascades
//        }

//    }
//}


using Microsoft.EntityFrameworkCore;
using QuizAPI.Models;

namespace QuizAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet properties for each model
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuizNode> QuizNodes { get; set; }
        public DbSet<Attempt> Attempts { get; set; }
        public DbSet<AttemptedQuestion> AttemptedQuestions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity
            modelBuilder.Entity<User>()
                .HasMany(u => u.Quizzes)
                .WithOne(q => q.Author)
                .HasForeignKey(q => q.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Attempts)
                .WithOne(a => a.Player)
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz Entity
            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(qn => qn.Quiz)
                .HasForeignKey(qn => qn.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizNode Entity
            //modelBuilder.Entity<QuizNode>()
            //    .OwnsOne(qn => qn.Location, loc =>
            //    {
            //        loc.Property(l => l.Type).HasDefaultValue("Point");
            //        loc.Property(l => l.Coordinates).HasConversion(
            //            v => string.Join(',', v),
            //            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList()
            //        );
            //    });

            modelBuilder.Entity<QuizNode>()
                .OwnsOne(q => q.Location)
                .HasIndex(l => l.Coordinates)
                .IsUnique(false);

            modelBuilder.Entity<QuizNode>()
                .HasMany(qn => qn.Quizzes)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            // Question Entity
            modelBuilder.Entity<Question>()
    .HasOne(q => q.Quiz)
    .WithMany(qz => qz.Questions)
    .HasForeignKey(q => q.QuizId)
    .OnDelete(DeleteBehavior.Cascade); // Allow cascade delete for Quiz

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Author)
                .WithMany()
                .HasForeignKey(q => q.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete for Author

            modelBuilder.Entity<Question>()
                .Property(q => q.Options)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<Question>()
                .Property(q => q.Answers)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                );

            // Attempt Entity
            // Correct relationship between Attempt and AttemptedQuestion
            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Player)
                .WithMany()
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete for Player

            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Quiz)
                .WithMany()
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Cascade); // Allow cascade delete for Quiz



            modelBuilder.Entity<Attempt>()
                .HasOne(a => a.Feedback)
                .WithOne(f => f.Attempt)
                .HasForeignKey<Feedback>(f => f.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // AttemptedQuestion Entity
            modelBuilder.Entity<AttemptedQuestion>()
                .Property(aq => aq.Answers)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
                );

            // Feedback Entity
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Attempt)
                .WithOne(a => a.Feedback)
                .HasForeignKey<Feedback>(f => f.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
