﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NotesApp.Api.Middlewares;
using NotesApp.Infrastructure.Data;

namespace NotesApp.IntegrationTests.Helpers
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        public IConfiguration Configuration { get; private set; }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                Configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.IntegrationTests.json")
                    .Build();

                config.AddConfiguration(Configuration);
            });

            builder.ConfigureServices(services =>
            {
                var mongoDbSettings = Configuration.GetSection("MongoDbSettings");
                var connectionString = mongoDbSettings.GetSection("ConnectionString").Value;
                var databaseName = mongoDbSettings.GetSection("DatabaseName").Value;

                services.AddSingleton<IMongoClient, MongoClient>(sp =>
                {
                    var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                    return new MongoClient(settings);
                });

                services.AddScoped(sp =>
                {
                    var client = sp.GetRequiredService<IMongoClient>();
                    var database = client.GetDatabase(databaseName); // use a test database for integration tests
                    return database;
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context (AppDbContext).
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<IMongoDatabase>();


                try
                {
                    Utilities.ReinitializeDbForTests(db);
                }
                catch (Exception ex)
                {
                    var logger = scopedServices.GetRequiredService<ILogger<ExceptionMiddleware>>();
                    logger.LogError($"Error occured while seeding data for Test Db: {ex}");
                }
            });

        

            builder.UseEnvironment("IntegrationTests");
        }
    }
}
