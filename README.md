# TempWatch2

โปรเจกต์เรียนรู้ ASP.NET Core Web API — ฝึกสร้าง API เก็บค่าอุณหภูมิและความชื้นลง SQL Server ด้วย Entity Framework Core

เป้าหมายไม่ใช่ระบบ production แต่เป็นที่ทดลองโครงสร้างแบบ Controller → Service → DbContext, DTO, validation, และ Swagger (มีคอมเมนต์เทียบกับ Go / Gin / GORM ไว้ในโค้ด)

## สิ่งที่ต้องมี

- .NET 10 SDK
- SQL Server LocalDB (ค่าเริ่มต้นใน `appsettings.json`)

## รันโปรเจกต์

จากโฟลเดอร์โปรเจกต์ API:

```powershell
cd TempWatch2
dotnet ef database update
dotnet run
```

เปิด Swagger ที่ [http://localhost:5078/swagger](http://localhost:5078/swagger)

หน้าแรก `/` จะ redirect ไป Swagger อัตโนมัติ

ปิดโปรเซสที่รันอยู่ (จาก root ของ repo):

```powershell
.\stop.ps1
```

## API คร่าวๆ

| Method | Path | ความหมาย |
| --- | --- | --- |
| `POST` | `/api/temperature` | บันทึกค่าอุณหภูมิ / ความชื้น |
| `GET` | `/api/temperature` | ประวัติทั้งหมด (ใหม่สุดก่อน) |
| `GET` | `/api/temperature/latest` | ค่าล่าสุด |
| `GET` | `/api/temperature/test` | แทรกค่าทดสอบแล้วคืนแถวที่สร้าง |
| `GET` | `/api/temperature/test2` | คืนค่าจำลอง ไม่แตะฐานข้อมูล |

ตัวอย่าง body ของ `POST /api/temperature`:

```json
{
  "temperature": 29.5,
  "humidity": 65
}
```
