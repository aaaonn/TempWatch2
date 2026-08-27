// class ที่ถูกแปลงเป็น JSON
namespace TempWatch2.Models
{
    public class TemperatureTestResponse
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}


//เทียบ Go:
// type TemperatureTestResponse struct {
// 	Temperature float64 `json:"temperature"`
// 	Humidity    float64 `json:"humidity"`
// }