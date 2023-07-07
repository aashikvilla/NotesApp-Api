using NotesApp.Domain.Entities;
using NotesApp.Infrastructure.Data;
using NotesApp.Application.Dto;
using NotesApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace NotesApp.IntegrationTests.Helpers
{
    public static class Utilities
    {
        public static UserLoginDto validUserLogin => new UserLoginDto
        {
            Email = "aashik@gmail.com",
            Password = "TestPassword123!"
        };

        public static void InitializeDbForTests(AppDbContext db)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                // Turn identity insert ON for Users
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users ON;");

                // Add users 
                db.Users.AddRange(GetSeedingUsers());
                db.SaveChanges();

                // Turn identity insert OFF for Users
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users OFF;");


                // Turn identity insert ON for Notes
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Notes ON;");

                // Add users 
                db.Notes.AddRange(GetSeedingNotes());
                db.SaveChanges();

                // Turn identity insert OFF for Notes
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Notes OFF;");

                transaction.Commit();
            }
        }


        public static void ReinitializeDbForTests(AppDbContext db)
        {
            db.Users.RemoveRange(db.Users);
            db.Notes.RemoveRange(db.Notes);
            InitializeDbForTests(db);
        }

        public static List<User> GetSeedingUsers()
        {
            var passwordHasher = new PasswordHasher();
            return new List<User>()
            {
                new User(){
                    Id = 1,
                    FirstName="Aashik",
                    LastName="Villa",
                    Email = validUserLogin.Email,
                    Password = passwordHasher.HashPassword(validUserLogin.Password)                  
                }
            };
        }

        public static List<Note> GetSeedingNotes()
        {
            return new List<Note>()
            {
                new Note()
                {
                    Id = 1,
                    Title = "Travel Plans",
                    Description = "note desc",
                    Priority = "LOW",
                    Status = "NOT STARTED",
                    UserId = 1
                },
                new Note()
                {
                    Id = 2,
                    Title = "Dinner Plan",
                    Description = "Reserve a table",
                    Priority = "HIGH",
                    Status = "COMPLETED",
                    UserId = 1
                }
            };
        }


    }
}
