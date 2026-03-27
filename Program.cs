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

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=zalo.db"));
builder.Services.AddHttpClient<ZaloTokenService>();
builder.Services.AddHttpClient<ZaloMessageService>();
builder.Services.AddScoped<GoogleSheetService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<FileStorageService>();

builder.Services.AddSingleton<TestIdOption>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();

