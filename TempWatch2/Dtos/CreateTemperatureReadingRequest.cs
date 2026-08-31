using System.ComponentModel.DataAnnotations;

namespace TempWatch2.Dtos
{
    // DTO = รูป JSON ที่ client ส่งเข้ามา ไม่ใช่แถวในตาราง
    // ต่างจาก Entity (TemperatureReading) ที่ต้องมี Id และ RecordedAt
    // Client ห้ามกำหนด Id / RecordedAt — backend ตั้งเอง (เทียบ GORM ที่ตั้ง CreatedAt ในโค้ด)
    //
    // ใช้ double? + [Required] ไม่ใช่ double
    // เพราะถ้าเป็น double แล้ว JSON ไม่ส่ง field มา จะกลายเป็น 0 แล้วผ่าน validation
    // ถ้าเป็น double? + [Required] ขาด field จะได้ 400
    public class CreateTemperatureReadingRequest
    {
        [Required]
        [Range(-40, 80)]
        public double? Temperature { get; set; }

        [Required]
        [Range(0, 100)]
        public double? Humidity { get; set; }
    }
}

// เทียบ Go:
// type CreateTemperatureReadingRequest struct {
//     Temperature *float64 `json:"temperature" binding:"required"`
//     Humidity    *float64 `json:"humidity" binding:"required"`
// }
