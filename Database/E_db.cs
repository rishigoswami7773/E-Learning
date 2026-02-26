using Microsoft.EntityFrameworkCore;
using Project_BD.Models;

namespace Project_BD.Database
{
    public class E_db : DbContext
    {
        public E_db(DbContextOptions options) : base(options)
        {
        }



        public DbSet<Student> Students { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Module> Modules { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Option> Options { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = null!;





    }
}
