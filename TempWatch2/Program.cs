using Microsoft.EntityFrameworkCore;
using TempWatch2.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ลงทะเบียน DbContext กับ DI — เวลา Controller ขอ TempWatchDbContext ระบบจะสร้างให้
builder.Services.AddDbContext<TempWatchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TempWatch")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();
