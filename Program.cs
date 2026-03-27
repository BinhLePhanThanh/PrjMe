using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.Configure<ZaloOptions>(
    builder.Configuration.GetSection("Zalo"));

var dbPath = Path.Combine(AppContext.BaseDirectory, "zalo.db");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddHttpClient<ZaloTokenService>();
builder.Services.AddHttpClient<ZaloMessageService>();
builder.Services.AddScoped<GoogleSheetService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<FileStorageService>();

builder.Services.AddSingleton<TestIdOption>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    Console.WriteLine("DB Path: " + db.Database.GetDbConnection().DataSource);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();

