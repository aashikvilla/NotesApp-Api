using NotesApp.Api.Middlewares;
using NotesApp.Application.Services.Users;
using NotesApp.Domain.RepositoryInterfaces;
using NotesApp.Infrastructure.Repositories;
using NotesApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotesApp.Application.Services.Notes;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
//Mongo Db setup
var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings");
var connectionString = mongoDbSettings.GetSection("ConnectionString").Value;
var databaseName = mongoDbSettings.GetSection("DatabaseName").Value;
var mongoClient = new MongoClient(connectionString);
var database = mongoClient.GetDatabase(databaseName);

builder.Services.AddSingleton(mongoClient);
builder.Services.AddSingleton<IMongoDatabase>(sp => sp.GetRequiredService<MongoClient>().GetDatabase(databaseName));

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Services.AddTransient<INoteService, NoteService>();
builder.Services.AddTransient<INoteRepository, NoteRepository>();

builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<ITokenService, TokenService>();

// JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings.GetSection("Secret").Value);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Add this
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
