// class ที่ถูกแปลงเป็น JSON
namespace TempWatch2.Models
{
    public class TemperatureTestResponse
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}


//เทียบ Go:
// type TemperatureTestResponse struct {
// 	Id          int       `json:"id"`
// 	Temperature float64   `json:"temperature"`
// 	Humidity    float64   `json:"humidity"`
// 	RecordedAt  time.Time `json:"recordedAt"`
// }
