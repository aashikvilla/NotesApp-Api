using NotesApp.Domain.Entities;
using NotesApp.Application.Dto;
using NotesApp.Infrastructure.Services;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using NotesApp.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using AutoFixture;
using MongoDB.Bson;
using System.Security.Policy;
using System.Web;
using NotesApp.Common.Models;
using System.Linq;
using System.Text;

namespace NotesApp.IntegrationTests.Helpers
{

    public static class Utilities
    {
        private static Fixture fixture = new Fixture();

        public static UserLoginDto validUserLogin = GenerateUserLoginDto();
        private static List<User> seedUsers = GenerateSeedingUsers();
        private static List<Note> seedNotes = GenerateSeedingNotes();
       
       

        public static void ReinitializeDbForTests(IMongoDatabase db, MongoDbSettings mongoDbSettings)
        {
            try
            {
                db.DropCollection(mongoDbSettings.UsersCollectionName);
                var users = db.GetCollection<User>(mongoDbSettings.UsersCollectionName);
                users.InsertMany(GetSeedingUsers());
                db.DropCollection(mongoDbSettings.NotesCollectionName);
                var notes = db.GetCollection<Note>(mongoDbSettings.NotesCollectionName);
                notes.InsertMany(GetSeedingNotes());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           
        }

        public static List<User> GetSeedingUsers()
        {
            return seedUsers;
        }

        public static List<Note> GetSeedingNotes()
        {
            return seedNotes;
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


        private static List<User> GenerateSeedingUsers()
        {
            var passwordHasher = new PasswordHasher();

            var validUser = fixture.Build<User>()
                .Without(x => x.Id)
                .Without(x => x.Password)
                .Do(x => x.Id = ObjectId.GenerateNewId().ToString())
                .With(x => x.Email, validUserLogin.Email)
                .With(x => x.Password, passwordHasher.HashPassword(validUserLogin.Password))
                .Create();

            var restUsers = fixture.Build<User>()
                .Without(x => x.Id)
                .Without(x => x.Password)
                .Do(x => x.Id = ObjectId.GenerateNewId().ToString())
                .Do(x => x.Password = passwordHasher.HashPassword( fixture.Create<string>()))
                .CreateMany(3).ToList();

            List<User> users = new List<User>() { validUser };
            users.AddRange(restUsers);
            return users;
        }

        private static List<Note> GenerateSeedingNotes()
        {
            List<Note> notes = seedUsers
                .SelectMany(user => fixture.Build<Note>()
                .Without(x => x.Id)
                .Do(x => x.Id = ObjectId.GenerateNewId().ToString())
                .With(x => x.UserId, user.Id)
                .CreateMany(30))
                .ToList();

            return notes;
        }

        private static UserLoginDto GenerateUserLoginDto()
        {
            return new UserLoginDto
            {
                Email = fixture.Create<string>(),
                Password = fixture.Create<string>()
            };
        }

        public static string GenerateQueryParameters(DataQueryParameters parameters)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query[nameof(DataQueryParameters.PageNumber)] = parameters.PageNumber.ToString();
            query[nameof(DataQueryParameters.PageSize)] = parameters.PageSize.ToString();

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query[nameof(DataQueryParameters.SearchTerm)]=parameters.SearchTerm;
            }
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query[nameof(DataQueryParameters.SortBy)] = parameters.SortBy;
            }
            if (!string.IsNullOrEmpty(parameters.SortOrder))
            {
                query[nameof(DataQueryParameters.SortOrder)] = parameters.SortOrder;
            }

            var queryString = new StringBuilder();
            queryString.Append( query.Count == 0 ? string.Empty : query.ToString()+"&" );

            foreach (var column in parameters.FilterColumns)
            {
                queryString.Append($"{nameof(DataQueryParameters.FilterColumns)}={Uri.EscapeDataString(column)}&");
            }
            foreach (var value in parameters.FilterQueries)
            {
                queryString.Append($"{nameof(DataQueryParameters.FilterQueries)}={Uri.EscapeDataString(value)}&");
            }

            return queryString.ToString();
        }
    }
}
