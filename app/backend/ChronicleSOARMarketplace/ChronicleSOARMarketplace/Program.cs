using ChronicleSOARMarketplace.Services;

var builder = WebApplication.CreateBuilder(args);

// Load configuration values (Pub/Sub subscription, Service Account path, VirusTotal API Key, etc.)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.

builder.Services.AddSingleton<EnrichmentService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HttpClient for VirusTotal Client
builder.Services.AddHttpClient("VirusTotalClient");

// Register custom services
builder.Services.AddSingleton<IVirusTotalClient, VirusTotalClient>();
builder.Services.AddHostedService<IngestionService>();
builder.Services.AddScoped<EnrichmentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
