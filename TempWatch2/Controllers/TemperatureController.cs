using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TempWatch2.Data;
using TempWatch2.Models;

namespace TempWatch2.Controllers
{
    [ApiController]
    [Route("api/temperature")]
    public class TemperatureController : ControllerBase
    {
        private readonly TempWatchDbContext _db;

        // DI ส่ง DbContext เข้ามาทาง constructor (ไม่ต้อง new เอง)
        public TemperatureController(TempWatchDbContext db)
        {
            _db = db;
        }

        [HttpGet("test")]
        public async Task<TemperatureTestResponse> GetTest()
        {
            // เขียนแถวทดสอบลง SQL Server แล้วอ่านแถวล่าสุดกลับมา
            _db.TemperatureReadings.Add(new TemperatureReading
            {
                Temperature = 29.5,
                Humidity = 65,
                RecordedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var latest = await _db.TemperatureReadings
                .OrderByDescending(r => r.Id)
                .FirstAsync();

            return new TemperatureTestResponse
            {
                Id = latest.Id,
                Temperature = latest.Temperature,
                Humidity = latest.Humidity,
                RecordedAt = latest.RecordedAt
            };
        }

        [HttpGet("test2")]
        public TemperatureTestResponse GetTest2()
        {
            // ค่าจำลองไว้เรียน request flow ยังไม่ได้อ่านจากเซ็นเซอร์หรือฐานข้อมูล
            return new TemperatureTestResponse
            {
                Temperature = 222,
                Humidity = 222
            };
        }
    }
}
