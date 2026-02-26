using Microsoft.EntityFrameworkCore;
using Project_BD.Models;

namespace Project_BD.Database
{
    public static class DbSeeder
    {
        public static void Seed(E_db context)
        {
            context.Database.EnsureCreated();

            // Ensure categories exist
            string[] catNames = { "Web Development", "Data Science", "Programming Languages", "Graphic Design" };
            string[] catDescs = {
                "Learn to build modern websites and web apps.",
                "Analyze data and build machine learning models.",
                "Master C#, Python, Java and more.",
                "Creative design principles and tools."
            };

            for (int i = 0; i < catNames.Length; i++)
            {
                if (!context.Categories.Any(c => c.Name == catNames[i]))
                {
                    context.Categories.Add(new Category { Name = catNames[i], Description = catDescs[i] });
                }
            }
            context.SaveChanges();

            var categories = context.Categories.ToDictionary(c => c.Name, c => c.CategoryId);

            int getCatId(string name) => categories.ContainsKey(name) ? categories[name] : categories.Values.FirstOrDefault();

            var coursesToSeed = new List<Course>
            {
                new Course { Title = "Master ASP.NET Core MVC", Description = "Learn how to build professional web applications using ASP.NET Core 8.", Price = 49.99m, CategoryId = getCatId("Web Development"), ThumbnailUrl = "/images/courses/aspnet.jpg", IsApproved = true },
                new Course { Title = "Complete React Developer", Description = "Master React.js from scratch including Hooks, Redux, and Context API.", Price = 59.99m, CategoryId = getCatId("Web Development"), ThumbnailUrl = "/images/courses/react.jpg", IsApproved = true },
                new Course { Title = "Python Core Mastery", Description = "Comprehensive guide to Python programming from variables to advanced decorators.", Price = 39.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/python.jpg", IsApproved = true },
                new Course { Title = "Java Programming: Zero to Hero", Description = "Learn Java syntax, OOP principles, and build real-world applications.", Price = 44.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/java.jpg", IsApproved = true },
                new Course { Title = "Modern JavaScript (ES6+)", Description = "Dominate the web with advanced JavaScript concepts and asynchronous programming.", Price = 34.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/js.jpg", IsApproved = true },
                new Course { Title = "C++ Essentials", Description = "Master memory management and performance-critical programming with C++.", Price = 49.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/cpp.jpg", IsApproved = true },
                new Course { Title = "Full Stack Web Development with MERN", Description = "Build scalable web applications using MongoDB, Express, React, and Node.js.", Price = 69.99m, CategoryId = getCatId("Web Development"), ThumbnailUrl = "/images/courses/mern.jpg", IsApproved = true },
                new Course { Title = "Big Data with Apache Spark", Description = "Learn to process massive datasets using Spark and Scala/Python.", Price = 79.99m, CategoryId = getCatId("Data Science"), ThumbnailUrl = "/images/courses/spark.jpg", IsApproved = true },
                new Course { Title = "Rust Programming for Systems", Description = "System-level programming with memory safety and high performance using Rust.", Price = 54.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/rust.jpg", IsApproved = true },
                new Course { Title = "Advanced Photoshop & Illustrator", Description = "Master industry-standard design tools for professional graphic design projects.", Price = 45.99m, CategoryId = getCatId("Graphic Design"), ThumbnailUrl = "/images/courses/design.jpg", IsApproved = true },
                new Course { Title = "Machine Learning with Python", Description = "Learn the math and implementation of ML algorithms using Scikit-Learn.", Price = 64.99m, CategoryId = getCatId("Data Science"), ThumbnailUrl = "/images/courses/ml.jpg", IsApproved = true }
            };

            foreach (var c in coursesToSeed)
            {
                if (!context.Courses.Any(existing => existing.Title == c.Title))
                {
                    context.Courses.Add(c);
                    context.SaveChanges();

                    // Seed details only for new courses
                    // Module 1: Introduction
                    var module1 = new Module { ModuleTitle = "Module 1: Getting Started with " + c.Title, CourseId = c.CourseId };
                    context.Modules.Add(module1);
                    context.SaveChanges();

                    context.Lessons.AddRange(new Lesson[]
                    {
                        new Lesson { LessonTitle = "Introduction and Setup", VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Content = "Welcome to the course! In this lesson, we'll set up our environment and understand the core objectives.", ModuleId = module1.ModuleId },
                        new Lesson { LessonTitle = "First Steps", VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Content = "Now we'll dive into the basic syntax and concepts to get you up and running quickly.", ModuleId = module1.ModuleId }
                    });

                    // Module 2: Intermediate Concepts
                    var module2 = new Module { ModuleTitle = "Module 2: Core Concepts in " + c.Title, CourseId = c.CourseId };
                    context.Modules.Add(module2);
                    context.SaveChanges();

                    context.Lessons.AddRange(new Lesson[]
                    {
                        new Lesson { LessonTitle = "Advanced Syntax and Features", VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Content = "We're moving beyond the basics to explore more powerful features of the language or tool.", ModuleId = module2.ModuleId },
                        new Lesson { LessonTitle = "Best Practices and Design Patterns", VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Content = "Learn how the pros write clean, maintainable code using industry-standard patterns.", ModuleId = module2.ModuleId }
                    });

                    var quiz = new Quiz { Title = c.Title + " Certification Quiz", CourseId = c.CourseId };
                    context.Quizzes.Add(quiz);
                    context.SaveChanges();

                    // Question 1
                    var q1 = new Question { QuestionText = "What is a primary advantage of " + c.Title + "?", QuizId = quiz.QuizId };
                    context.Questions.Add(q1);
                    context.SaveChanges();

                    context.Options.AddRange(new Option[]
                    {
                        new Option { OptionText = "Industry Standard Performance", IsCorrect = true, QuestionId = q1.QuestionId },
                        new Option { OptionText = "Easier than everything else", IsCorrect = false, QuestionId = q1.QuestionId },
                        new Option { OptionText = "It is free to use", IsCorrect = false, QuestionId = q1.QuestionId }
                    });

                    // Question 2
                    var q2 = new Question { QuestionText = "Which of the following is a core concept in " + c.Title + "?", QuizId = quiz.QuizId };
                    context.Questions.Add(q2);
                    context.SaveChanges();

                    context.Options.AddRange(new Option[]
                    {
                        new Option { OptionText = "Scalability and Efficiency", IsCorrect = true, QuestionId = q2.QuestionId },
                        new Option { OptionText = "Ignoring errors", IsCorrect = false, QuestionId = q2.QuestionId },
                        new Option { OptionText = "Manual data entry", IsCorrect = false, QuestionId = q2.QuestionId }
                    });
                }
            }

            return;
        }
    }
}
