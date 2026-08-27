using Microsoft.EntityFrameworkCore;
using TempWatch2.Models;

namespace TempWatch2.Data
{
    // DbContext = ประตูเข้าออกฐานข้อมูล (คล้าย *gorm.DB)
    public class TempWatchDbContext : DbContext
    {
        public TempWatchDbContext(DbContextOptions<TempWatchDbContext> options)
            : base(options)
        {
        }

        // DbSet = ตาราง TemperatureReadings (EF จะใส่ค่าให้ตอนสร้าง context)
        public DbSet<TemperatureReading> TemperatureReadings { get; set; } = null!;
    }
}
