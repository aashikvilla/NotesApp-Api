using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using NotesApp.Api.Middlewares;
using NotesApp.Application.Services.Notes;
using NotesApp.Application.Services.Users;
using NotesApp.Domain.RepositoryInterfaces;
using NotesApp.Infrastructure.Data;
using NotesApp.Infrastructure.Repositories;
using NotesApp.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Mongo Db setup
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

MongoDbSettings mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
builder.Services.AddSingleton<IMongoClient, MongoClient>(_ =>
{
    return new MongoClient(mongoDbSettings?.ConnectionString);
});
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
});

// Add CORS services to the services container
var corsPolicy = builder.Configuration.GetValue<string>("CorsPolicy");

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy,
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token below:",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();


app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseCors(corsPolicy);


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
