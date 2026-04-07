using System.Data.Common;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using Serilog.Events;
using SomeBoard.Backend.Context;
using SomeBoard.Shared.Exceptions;

namespace SomeBoard.Backend;

public class Program
{
    public const string CONFIG_PATH = "SomeBoard";
    public const string POSTING_CONNECTIONSTRING_PATH = $"{CONFIG_PATH}:Posting:ConnectionString";
    public const string POSTING_DATABASE_PATH = $"{CONFIG_PATH}:Posting:Database";
    public const string POSTING_USERNAME_PATH = $"{CONFIG_PATH}:Posting:Username";
    public const string POSTING_PASSWORD_PATH = $"{CONFIG_PATH}:Posting:Password";
    public const string POSTING_PASSFILE_PATH = $"{CONFIG_PATH}:Posting:PasswordFile";
    public const string POSTING_HOST_PATH = $"{CONFIG_PATH}:Posting:Host";
    public const string POSTING_PORT_PATH = $"{CONFIG_PATH}:Posting:Port";
    public const string POSTING_ADMIN_SECRETFILE_PATH = $"{CONFIG_PATH}:Posting:AdminSecretFile";
    [Pure]
    public static string BuildPath(string name, string path = CONFIG_PATH) => $"{path}:{name}";
    
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.File("./logs/SomeBoard.Backend.log", 
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
        var builder = WebApplication.CreateBuilder(args);

        AddPostingContext(builder);

        // Add services to the container.
        builder.Services.AddSerilog();
        // builder.Services.AddAuthorization(); // right now backend does not use authorization

        builder.Services.AddControllers();
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();
        
        //Enable request logging
        app.UseSerilogRequestLogging();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi("/openapi.json");
        }

        app.UseHttpsRedirection();

        // app.UseAuthorization(); // right now backend does not use authorization

        app.MapControllers();

        app.Run();
    }

    private static void AddPostingContext(WebApplicationBuilder builder)
    {
        var conString = builder.Configuration.GetValue<string>(POSTING_CONNECTIONSTRING_PATH);
        bool usingPassfile = false;
        if (string.IsNullOrEmpty(conString))
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
            {
                Database = builder.Configuration.GetValue<string>(POSTING_DATABASE_PATH, "someboard"),
                Host = builder.Configuration.GetValue<string?>(POSTING_HOST_PATH, null),
                Port = builder.Configuration.GetValue<int>(POSTING_PORT_PATH, 5432),
                Username = builder.Configuration.GetValue<string?>(POSTING_USERNAME_PATH, null),
                
                Passfile = new Func<string?>(() =>
                {
                    var a = builder.Configuration.GetValue<string?>(POSTING_PASSFILE_PATH, null);
                    usingPassfile = a is not null;
                    return a;
                }).Invoke(),
                Password = new Func<string?>(() =>
                {
                    if (usingPassfile) return null;
                    return builder.Configuration.GetValue<string?>(POSTING_PASSWORD_PATH, null);
                }).Invoke()
            };
            if (connectionStringBuilder.Host is null) throw MissingValue(POSTING_HOST_PATH);
            if (connectionStringBuilder.Username is null) throw MissingValue(POSTING_USERNAME_PATH);
            if (connectionStringBuilder.Passfile is null && connectionStringBuilder.Password is null)
                throw MissingValue($"{POSTING_PASSFILE_PATH} and {POSTING_PASSWORD_PATH}");
            conString = connectionStringBuilder.ToString();
        }

        builder.Services.AddDbContext<PostingContext>(options =>
        {
            options.UseNpgsql(conString);
        }, ServiceLifetime.Singleton);

        EmptyNotAllowedException MissingValue(string name)
        {
            return new EmptyNotAllowedException($"Database initialisation failed: Missing {name} from configuration.");
        }
    }

    public const string AdminSecretsDefaultFile = "adminsecrets.txt";
    public static bool VerifyAdminSecret(string secret, IConfiguration config)
    {
        return File.ReadAllText(config.GetValue<string>(POSTING_ADMIN_SECRETFILE_PATH, 
            Path.Combine(Directory.GetCurrentDirectory(), AdminSecretsDefaultFile))) == secret;
    }
}