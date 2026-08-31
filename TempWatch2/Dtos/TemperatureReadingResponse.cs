namespace TempWatch2.Dtos
{
    // JSON ที่ API ส่งกลับ — มี Id และ RecordedAt ที่ backend สร้างให้แล้ว
    public class TemperatureReadingResponse
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}

// เทียบ Go:
// type TemperatureReadingResponse struct {
//     Id          int       `json:"id"`
//     Temperature float64   `json:"temperature"`
//     Humidity    float64   `json:"humidity"`
//     RecordedAt  time.Time `json:"recordedAt"`
// }
