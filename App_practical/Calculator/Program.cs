using Calculator.Data;
using Calculator.Services;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CalculatorContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        options => options.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)
    ));

// Add HttpClient for callback
builder.Services.AddHttpClient();

// Add Kafka services like in the example
builder.Services.AddSingleton<KafkaProducerHandler>();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

// Логирование конфигурации Kafka
var kafkaBootstrapServers = builder.Configuration["Kafka:ConsumerSettings:BootstrapServers"];
var kafkaGroupId = builder.Configuration["Kafka:ConsumerSettings:GroupId"];
var kafkaTopic = builder.Configuration["Kafka:TopicName"];

Console.WriteLine($"=== Kafka Configuration ===");
Console.WriteLine($"BootstrapServers: {kafkaBootstrapServers}");
Console.WriteLine($"GroupId: {kafkaGroupId}");
Console.WriteLine($"Topic: {kafkaTopic}");

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CalculatorContext>();

        // Wait for database to be ready
        var retries = 12;
        for (int i = 1; i <= retries; i++)
        {
            try
            {
                Console.WriteLine($"Testing database connection... Attempt {i}/{retries}");

                if (context.Database.CanConnect())
                {
                    Console.WriteLine("✅ Database connection successful!");

                    // Apply migrations
                    Console.WriteLine("Applying database migrations...");
                    context.Database.Migrate();
                    Console.WriteLine("✅ Database migrations applied successfully!");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed (attempt {i}): {ex.Message}");
                if (i == retries)
                {
                    Console.WriteLine("💥 FATAL: Failed to connect to database after all retries");
                    throw;
                }
                Console.WriteLine("Waiting 10 seconds before next attempt...");
                Thread.Sleep(10000);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while migrating the database: {ex.Message}");
        throw;
    }
}

Console.WriteLine("✅ Database initialization completed successfully!");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("✅ Application startup completed!");

app.Run();
