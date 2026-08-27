using Microsoft.AspNetCore.Mvc;
using TempWatch2.Models;

namespace TempWatch2.Controllers
{
    [ApiController]
    [Route("api/temperature")]
    public class TemperatureController : ControllerBase
    {
        [HttpGet("test")]
        public TemperatureTestResponse GetTest()
        {
            // ค่าจำลองไว้เรียน request flow ยังไม่ได้อ่านจากเซ็นเซอร์หรือฐานข้อมูล
            return new TemperatureTestResponse
            {
                Temperature = 29.5,
                Humidity = 65
            };
        }
    }
}
