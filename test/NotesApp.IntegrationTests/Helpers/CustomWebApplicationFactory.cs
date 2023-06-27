using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotesApp.Api.Middlewares;
using NotesApp.Infrastructure.Data;

namespace NotesApp.IntegrationTests.Helpers
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly ILogger<ExceptionMiddleware> _logger;
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
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Use SQL Server for the tests and get connection string from appsettings.test.json
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context (AppDbContext).
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                // Ensure the database is created.
                db.Database.EnsureCreated();

                try
                {
                    Utilities.ReinitializeDbForTests(db);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error occured while seeding data for Test Db: {ex}");
                }
            });

        

            builder.UseEnvironment("IntegrationTests");
        }
    }
}
