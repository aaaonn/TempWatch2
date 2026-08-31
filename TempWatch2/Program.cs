using Microsoft.EntityFrameworkCore;
using TempWatch2.Data;
using TempWatch2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ลงทะเบียน DbContext กับ DI — เวลา Controller ขอ TempWatchDbContext ระบบจะสร้างให้
builder.Services.AddDbContext<TempWatchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TempWatch")));

// AddScoped = อายุเท่าหนึ่ง request (DbContext ก็ scoped)
// เทียบ Gin: ต่อ *gorm.DB ให้ handler ใช้ต่อ request ไม่แชร์ข้าม request
builder.Services.AddScoped<TemperatureService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();
