using Microsoft.EntityFrameworkCore;
using TempWatch2.Data;
using TempWatch2.Dtos;
using TempWatch2.Models;

namespace TempWatch2.Services
{
    // ชั้น Service = ตรรกะธุรกิจ + คุยกับฐานข้อมูล
    // เทียบ Go: struct ที่รับ *gorm.DB แล้วมี method Create / Find
    // Controller ไม่เรียก DbContext ตรงๆ — เหลือแค่รับ HTTP แล้วส่งต่อ
    public class TemperatureService
    {
        private readonly TempWatchDbContext _db;

        public TemperatureService(TempWatchDbContext db)
        {
            _db = db;
        }

        // เทียบ GORM: db.Create(&reading)
        // Task = "งานที่ยังไม่เสร็จ" — คล้ายฟังก์ชันที่รับ context.Context ใน Go
        // async/await = รอ SQL โดยไม่บล็อก thread (Go มัก goroutine + err return)
        public async Task<TemperatureReadingResponse> CreateAsync(CreateTemperatureReadingRequest request)
        {
            var entity = new TemperatureReading
            {
                Temperature = request.Temperature!.Value,
                Humidity = request.Humidity!.Value,
                // ใช้ DateTime.Now เพื่อให้ตรงนาฬิกาเครื่อง (ไทย UTC+7)
                // API จริงหลายตัวเก็บ UTC แล้วให้ client แปลง — โปรเจกต์นี้เลือกแบบอ่านง่ายก่อน
                RecordedAt = DateTime.Now
            };

            _db.TemperatureReadings.Add(entity);
            await _db.SaveChangesAsync();

            return ToResponse(entity);
        }

        // เทียบ GORM: db.Order("recorded_at desc").First(&reading)
        // FirstOrDefaultAsync คืน null ถ้ายังไม่มีแถว (Go มัก ErrRecordNotFound)
        public async Task<TemperatureReadingResponse?> GetLatestAsync()
        {
            var latest = await _db.TemperatureReadings
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (latest == null)
            {
                return null;
            }

            return ToResponse(latest);
        }

        // เทียบ GORM: db.Order("recorded_at desc").Find(&readings)
        // ว่างได้ — คืน list ว่าง ไม่ใช่ null (history ใช้ 200 [])
        public async Task<List<TemperatureReadingResponse>> GetAllAsync()
        {
            var rows = await _db.TemperatureReadings
                .OrderByDescending(r => r.RecordedAt)
                .ToListAsync();

            return rows.Select(ToResponse).ToList();
        }

        private static TemperatureReadingResponse ToResponse(TemperatureReading entity)
        {
            return new TemperatureReadingResponse
            {
                Id = entity.Id,
                Temperature = entity.Temperature,
                Humidity = entity.Humidity,
                RecordedAt = entity.RecordedAt
            };
        }
    }
}
