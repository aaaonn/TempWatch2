# TempWatch2

โปรเจกต์เรียนรู้ ASP.NET Core Web API — ฝึกสร้าง API เก็บค่าอุณหภูมิและความชื้นลง SQL Server ด้วย Entity Framework Core

เป้าหมายไม่ใช่ระบบ production แต่เป็นที่ทดลองโครงสร้างแบบ Controller → Service → DbContext, DTO, validation, และ Swagger (มีคอมเมนต์เทียบกับ Go / Gin / GORM ไว้ในโค้ด)

## สิ่งที่ต้องมี

- .NET 10 SDK
- SQL Server LocalDB (ค่าเริ่มต้นใน `appsettings.json`)
- Flutter SDK (สำหรับแอปใน `tempwatch_app/`)

## รันโปรเจกต์

จากโฟลเดอร์โปรเจกต์ API:

```powershell
cd TempWatch2
dotnet ef database update
dotnet run
```

เปิด Swagger ที่ [http://localhost:5078/swagger](http://localhost:5078/swagger)

(API listen ที่ `0.0.0.0:5078` เพื่อให้เครื่องอื่นใน LAN เรียกได้ — เปิดในเบราว์เซอร์ยังใช้ `localhost` ได้เหมือนเดิม)

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

## รันแอป Flutter

ต้องมี [Flutter SDK](https://docs.flutter.dev/get-started/install) แล้วรัน API ก่อน (`dotnet run` ที่พอร์ต 5078)

```powershell
cd tempwatch_app
flutter pub get
flutter run -d windows
```

หรือเลือกอุปกรณ์ Android (`flutter devices` แล้ว `flutter run -d <id>`)

Base URL อยู่ที่ `tempwatch_app/lib/api_config.dart` — เปลี่ยนบรรทัดเดียวตามเป้าทดสอบ:

| เป้าทดสอบ | `ApiConfig.baseUrl` | หมายเหตุ |
| --- | --- | --- |
| Flutter Windows | `http://localhost:5078` | ค่าเริ่มต้น |
| Android Emulator | `http://10.0.2.2:5078` | `10.0.2.2` คือ localhost ของเครื่อง host — emulator มอง `localhost` เป็นตัวเอง |
| โทรศัพท์จริงใน LAN | `http://<LAN-IP-ของ-PC>:5078` | เช่น `http://192.168.1.10:5078` |

ข้อมูลยังใส่ผ่าน Postman / Swagger (`POST /api/temperature`) แอป Flutter อ่านอย่างเดียว (GET)

เมื่อใช้โทรศัพท์จริง: เปิด Windows Firewall ให้พอร์ต 5078 และให้ PC กับโทรศัพท์อยู่ Wi-Fi เดียวกัน
