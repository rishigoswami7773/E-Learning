using Microsoft.EntityFrameworkCore;
using Project_BD.Models;

namespace Project_BD.Database
{
    public class E_db : DbContext
    {
        public E_db(DbContextOptions options) : base(options)
        {
        }

       

        public DbSet<Student> Students { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Review> Reviews { get; set; }





    }
}
