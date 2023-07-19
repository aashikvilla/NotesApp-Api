using NotesApp.Domain.Entities;
using NotesApp.Application.Dto;
using NotesApp.Infrastructure.Services;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using NotesApp.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace NotesApp.IntegrationTests.Helpers
{
    public static class Utilities
    {
        public static UserLoginDto validUserLogin => new UserLoginDto
        {
            Email = "aashik@gmail.com",
            Password = "TestPassword123!"
        };


        public static void ReinitializeDbForTests(IMongoDatabase db, MongoDbSettings mongoDbSettings)
        {          
            db.DropCollection(mongoDbSettings.UsersCollectionName);
            var users = db.GetCollection<User>(mongoDbSettings.UsersCollectionName);
            users.InsertMany(GetSeedingUsers());
            db.DropCollection(mongoDbSettings.NotesCollectionName);
            var notes = db.GetCollection<Note>(mongoDbSettings.NotesCollectionName);
            notes.InsertMany(GetSeedingNotes());          
        }

        public static List<User> GetSeedingUsers()
        {
            var passwordHasher = new PasswordHasher();
            return new List<User>()
            {
                new User(){
                    Id = "64b6f787ff6d89b317265d22",
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
                    Id = "64b702c4576ee1a2851b73a9",
                    Title = "Travel Plans",
                    Description = "note desc",
                    Priority = "LOW",
                    Status = "NOT STARTED",
                    UserId = "64b6f787ff6d89b317265d22"
                },
                new Note()
                {
                    Id = "64b7038f0568a90a367709d8",
                    Title = "Dinner Plan",
                    Description = "Reserve a table",
                    Priority = "HIGH",
                    Status = "COMPLETED",
                    UserId = "64b6f787ff6d89b317265d22"
                }
            };
        }



        public static void ReinitializeDb(CustomWebApplicationFactory<Program> factory)
        {
            using (var scope = factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<IMongoDatabase>();

                var configuration = scopedServices.GetRequiredService<IConfiguration>();
                var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();

                Utilities.ReinitializeDbForTests(db, mongoDbSettings);
            }
        }


    }
}
