
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        _ = builder.Services.AddControllers();
        _ = builder.Services.AddScoped<IUserRepository, UserRepository>();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen();
        _ = builder.Services.AddDbContext<ChessDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                   .EnableSensitiveDataLogging() // Useful for debugging
                   .LogTo(Console.WriteLine));   // Logs SQL queries to the console

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI();
        }

        _ = app.UseHttpsRedirection();

        _ = app.UseAuthorization();


        _ = app.MapControllers();

        app.Run();
    }
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    _ = builder.AllowAnyOrigin() // Allow any origin
                           .AllowAnyMethod() // Allow any HTTP method (GET, POST, etc.)
                           .AllowAnyHeader(); // Allow any header
                });
        });

        // Other service configurations
        _ = services.AddControllers();
    }
}
