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
                    context.Categories.Add(new Category { Name = catNames[i], Description = catDescs[i] });
            }
            context.SaveChanges();

            var categories = context.Categories.ToDictionary(c => c.Name, c => c.CategoryId);
            int getCatId(string name) => categories.ContainsKey(name) ? categories[name] : categories.Values.FirstOrDefault();

            var coursesToSeed = new List<Course>
            {
                new Course { Title = "Master ASP.NET Core MVC",            Description = "Learn how to build professional web applications using ASP.NET Core 8.",                   Price = 49.99m, CategoryId = getCatId("Web Development"),        ThumbnailUrl = "/images/courses/aspnet.jpg",  IsApproved = true },
                new Course { Title = "Complete React Developer",            Description = "Master React.js from scratch including Hooks, Redux, and Context API.",                     Price = 59.99m, CategoryId = getCatId("Web Development"),        ThumbnailUrl = "/images/courses/react.jpg",   IsApproved = true },
                new Course { Title = "Python Core Mastery",                 Description = "Comprehensive guide to Python programming from variables to advanced decorators.",           Price = 39.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/python.jpg",  IsApproved = true },
                new Course { Title = "Java Programming: Zero to Hero",      Description = "Learn Java syntax, OOP principles, and build real-world applications.",                    Price = 44.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/java.jpg",    IsApproved = true },
                new Course { Title = "Modern JavaScript (ES6+)",            Description = "Dominate the web with advanced JavaScript concepts and asynchronous programming.",          Price = 34.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/js.jpg",      IsApproved = true },
                new Course { Title = "C++ Essentials",                      Description = "Master memory management and performance-critical programming with C++.",                   Price = 49.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/cpp.jpg",     IsApproved = true },
                new Course { Title = "Full Stack Web Development with MERN",Description = "Build scalable web applications using MongoDB, Express, React, and Node.js.",               Price = 69.99m, CategoryId = getCatId("Web Development"),        ThumbnailUrl = "/images/courses/mern.jpg",    IsApproved = true },
                new Course { Title = "Big Data with Apache Spark",          Description = "Learn to process massive datasets using Spark and Scala/Python.",                          Price = 79.99m, CategoryId = getCatId("Data Science"),           ThumbnailUrl = "/images/courses/spark.jpg",   IsApproved = true },
                new Course { Title = "Rust Programming for Systems",        Description = "System-level programming with memory safety and high performance using Rust.",              Price = 54.99m, CategoryId = getCatId("Programming Languages"), ThumbnailUrl = "/images/courses/rust.jpg",    IsApproved = true },
                new Course { Title = "Advanced Photoshop & Illustrator",    Description = "Master industry-standard design tools for professional graphic design projects.",           Price = 45.99m, CategoryId = getCatId("Graphic Design"),         ThumbnailUrl = "/images/courses/design.jpg",  IsApproved = true },
                new Course { Title = "Machine Learning with Python",        Description = "Learn the math and implementation of ML algorithms using Scikit-Learn.",                   Price = 64.99m, CategoryId = getCatId("Data Science"),           ThumbnailUrl = "/images/courses/ml.jpg",      IsApproved = true }
            };

            foreach (var c in coursesToSeed)
            {
                if (!context.Courses.Any(existing => existing.Title == c.Title))
                {
                    context.Courses.Add(c);
                    context.SaveChanges();

                    // Module 1
                    var module1 = new Module { ModuleTitle = "Module 1: Getting Started with " + c.Title, CourseId = c.CourseId };
                    context.Modules.Add(module1);
                    context.SaveChanges();
                    context.Lessons.AddRange(
                        new Lesson { LessonTitle = "Introduction and Setup",  VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Content = "Welcome to the course! In this lesson, we'll set up our environment and understand the core objectives.", ModuleId = module1.ModuleId },
                        new Lesson { LessonTitle = "First Steps",             VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Content = "Now we'll dive into the basic syntax and concepts to get you up and running quickly.",                    ModuleId = module1.ModuleId }
                    );

                    // Module 2
                    var module2 = new Module { ModuleTitle = "Module 2: Core Concepts in " + c.Title, CourseId = c.CourseId };
                    context.Modules.Add(module2);
                    context.SaveChanges();
                    context.Lessons.AddRange(
                        new Lesson { LessonTitle = "Advanced Syntax and Features",       VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Content = "We're moving beyond the basics to explore more powerful features.", ModuleId = module2.ModuleId },
                        new Lesson { LessonTitle = "Best Practices and Design Patterns", VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Content = "Learn how the pros write clean, maintainable code.",                  ModuleId = module2.ModuleId }
                    );
                    context.SaveChanges();

                    // Quiz with 5 questions
                    var quiz = new Quiz { Title = c.Title + " Certification Quiz", CourseId = c.CourseId };
                    context.Quizzes.Add(quiz);
                    context.SaveChanges();

                    SeedQuestionsForQuiz(context, quiz.QuizId, c.Title, 0);
                }
            }

            // ---------------------------------------------------------------
            // REPAIR: Ensure every existing quiz has at least 5 questions
            // ---------------------------------------------------------------
            var allQuizzes = context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Course)
                .ToList();

            foreach (var quiz in allQuizzes)
            {
                int existingCount = quiz.Questions?.Count ?? 0;
                if (existingCount < 5)
                {
                    string courseTitle = quiz.Course?.Title ?? "this subject";
                    SeedQuestionsForQuiz(context, quiz.QuizId, courseTitle, existingCount);
                }
            }
        }

        // ---------------------------------------------------------------
        // Seeds questions starting at 'startFrom' index up to 5 total
        // ---------------------------------------------------------------
        private static void SeedQuestionsForQuiz(E_db context, int quizId, string courseTitle, int startFrom)
        {
            var templates = GetQuestionTemplates(courseTitle);
            int end = Math.Min(5, templates.Count);

            for (int i = startFrom; i < end; i++)
            {
                var tpl = templates[i];
                var question = new Question { QuestionText = tpl.Text, QuizId = quizId };
                context.Questions.Add(question);
                context.SaveChanges();

                foreach (var opt in tpl.Options)
                {
                    context.Options.Add(new Option
                    {
                        OptionText = opt.Text,
                        IsCorrect  = opt.IsCorrect,
                        QuestionId = question.QuestionId
                    });
                }
                context.SaveChanges();
            }
        }

        private static List<QuestionTemplate> GetQuestionTemplates(string topic)
        {
            return new List<QuestionTemplate>
            {
                new QuestionTemplate
                {
                    Text = $"What is a primary advantage of learning {topic}?",
                    Options = new[]
                    {
                        new OptionTemplate("Industry-standard performance and reliability", true),
                        new OptionTemplate("It requires absolutely no prior knowledge",    false),
                        new OptionTemplate("It is always completely free to use",          false),
                        new OptionTemplate("It replaces all other technologies",           false)
                    }
                },
                new QuestionTemplate
                {
                    Text = $"Which of the following is a core concept in {topic}?",
                    Options = new[]
                    {
                        new OptionTemplate("Scalability and code efficiency",         true),
                        new OptionTemplate("Ignoring software errors at runtime",     false),
                        new OptionTemplate("Manual data entry without automation",    false),
                        new OptionTemplate("Avoiding reusable components entirely",   false)
                    }
                },
                new QuestionTemplate
                {
                    Text = $"What is the best practice when working with {topic}?",
                    Options = new[]
                    {
                        new OptionTemplate("Writing clean, modular, and documented code",  true),
                        new OptionTemplate("Copy-pasting code without understanding it",   false),
                        new OptionTemplate("Skipping testing entirely to save time",       false),
                        new OptionTemplate("Hardcoding all configuration values",          false)
                    }
                },
                new QuestionTemplate
                {
                    Text = $"Which environment is commonly used when developing with {topic}?",
                    Options = new[]
                    {
                        new OptionTemplate("An integrated development environment (IDE)", true),
                        new OptionTemplate("A basic text editor with no syntax support",  false),
                        new OptionTemplate("A spreadsheet application",                   false),
                        new OptionTemplate("A graphics design tool only",                 false)
                    }
                },
                new QuestionTemplate
                {
                    Text = $"What is the recommended way to handle errors in {topic}?",
                    Options = new[]
                    {
                        new OptionTemplate("Use structured error handling and logging",    true),
                        new OptionTemplate("Ignore all errors during development",         false),
                        new OptionTemplate("Crash the application to alert the user",      false),
                        new OptionTemplate("Delete the code section causing the error",    false)
                    }
                }
            };
        }

        // Inner helper classes
        private class QuestionTemplate
        {
            public string Text { get; set; } = "";
            public OptionTemplate[] Options { get; set; } = Array.Empty<OptionTemplate>();
        }

        private class OptionTemplate
        {
            public string Text     { get; }
            public bool IsCorrect  { get; }
            public OptionTemplate(string text, bool isCorrect) { Text = text; IsCorrect = isCorrect; }
        }
    }
}
