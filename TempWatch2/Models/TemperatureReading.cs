// Entity = class ที่ map กับตารางใน SQL Server (เทียบ GORM model)
namespace TempWatch2.Models
{
    public class TemperatureReading
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}

// เทียบ Go + GORM:
// type TemperatureReading struct {
//     ID          uint      `gorm:"primaryKey"`
//     Temperature float64
//     Humidity    float64
//     RecordedAt  time.Time
// }
