using NotesApp.Domain.Entities;
using NotesApp.Infrastructure.Data;
using NotesApp.Application.Dto;
using NotesApp.Infrastructure.Services;

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
            db.Users.AddRange(GetSeedingUsers());
            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(AppDbContext db)
        {
            db.Users.RemoveRange(db.Users);
            InitializeDbForTests(db);
        }

        public static List<User> GetSeedingUsers()
        {
            var passwordHasher = new PasswordHasher();
            return new List<User>()
            {
                new User(){
                    FirstName="Aashik",
                    LastName="Villa",
                    Email = validUserLogin.Email,
                    Password = passwordHasher.HashPassword(validUserLogin.Password),
                },
            };
        }
    }
}
