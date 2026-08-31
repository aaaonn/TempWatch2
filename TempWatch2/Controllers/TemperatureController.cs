using Microsoft.AspNetCore.Mvc;
using TempWatch2.Dtos;
using TempWatch2.Models;
using TempWatch2.Services;

namespace TempWatch2.Controllers
{
    // เทียบ Gin: router group "/api/temperature" + handler functions
    // Controller บางลงเหลือรับ HTTP แล้วส่งต่อ Service — ไม่มี LINQ / SaveChanges ที่นี่
    [ApiController]
    [Route("api/temperature")]
    public class TemperatureController : ControllerBase
    {
        private readonly TemperatureService _service;

        public TemperatureController(TemperatureService service)
        {
            _service = service;
        }

        // POST /api/temperature
        // เทียบ Gin: router.POST("/", handler)
        // [ApiController] แปลง validation fail เป็น 400 อัตโนมัติ ไม่ต้องเขียน if
        [HttpPost]
        public async Task<IActionResult> Create(CreateTemperatureReadingRequest request)
        {
            var created = await _service.CreateAsync(request);

            // 201 Created + Location ชี้ไปที่ GET latest (ยังไม่มี GET ตาม id)
            return CreatedAtAction(nameof(GetLatest), created);
        }

        // GET /api/temperature/latest
        // มีข้อมูล: 200 — ตารางว่าง: 404 (ต่างจาก history ที่คืน [])
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var latest = await _service.GetLatestAsync();
            if (latest == null)
            {
                return NotFound();
            }

            return Ok(latest);
        }

        // GET /api/temperature — ประวัติทั้งหมด ใหม่สุดก่อน
        // ว่าง: 200 + [] ไม่ใช้ 404 (404 สงวนไว้ให้ latest)
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _service.GetAllAsync();
            return Ok(history);
        }

        [HttpGet("test")]
        public async Task<TemperatureReadingResponse> GetTest()
        {
            // พฤติกรรมเดิม Phase 2: แทรก 29.5/65 แล้วคืนแถวที่เพิ่งสร้าง — ตอนนี้ผ่าน Service
            return await _service.CreateAsync(new CreateTemperatureReadingRequest
            {
                Temperature = 29.5,
                Humidity = 65
            });
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
