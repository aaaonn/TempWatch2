# TempWatch2

โปรเจกต์เรียนรู้ ASP.NET Core Web API — ฝึกสร้าง API เก็บค่าอุณหภูมิและความชื้นลง SQL Server ด้วย Entity Framework Core แล้วให้ Flutter และ ESP32 + DHT22 ส่ง/อ่านค่าผ่าน REST ชุดเดียวกัน

เป้าหมายไม่ใช่ระบบ production แต่เป็นที่ทดลองโครงสร้างแบบ Controller → Service → DbContext, DTO, validation, และ Swagger (มีคอมเมนต์เทียบกับ Go / Gin / GORM ไว้ในโค้ด)

```
DHT22 → ESP32 → HTTP POST /api/temperature → ASP.NET Core → SQL Server → Flutter (GET + Refresh)
```

ข้อมูลใส่ได้ทั้ง Postman / Swagger และ ESP32 (`POST`) แอป Flutter อ่านอย่างเดียว (`GET`)

---

## สิ่งที่ต้องมี

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (ค่าเริ่มต้นใน `TempWatch2/appsettings.json`)
- [Flutter SDK](https://docs.flutter.dev/get-started/install) — สำหรับแอปใน `tempwatch_app/`
- Arduino IDE + บอร์ด ESP32 + เซ็นเซอร์ DHT22 — สำหรับเฟิร์มแวร์ใน `firmware/` (ดูรายละเอียดที่ [`firmware/README.md`](firmware/README.md))

---

## โครงสร้างโฟลเดอร์

| โฟลเดอร์ | ทำอะไร |
| --- | --- |
| `TempWatch2/` | ASP.NET Core Web API + EF Core |
| `tempwatch_app/` | แอป Flutter (Dashboard / History) |
| `firmware/` | สเก็ตช์ ESP32 + DHT22 |

---

## 1. รัน API (`dotnet run`)

ทุกอย่างเริ่มจาก backend ต้องขึ้นก่อน Flutter และ ESP32 จึงจะคุยได้

เปิด PowerShell แล้วไปที่โฟลเดอร์โปรเจกต์ API:

```powershell
cd TempWatch2
```

สร้าง / อัปเดตฐานข้อมูล LocalDB ให้ตรงกับ migration ล่าสุด (ครั้งแรก หรือหลังมี migration ใหม่):

```powershell
dotnet ef database update
```

จากนั้นเปิด API:

```powershell
dotnet run
```

รอจนเห็นว่า listen ที่พอร์ต **5078** แล้วเปิดเบราว์เซอร์ไปที่:

- Swagger: [http://localhost:5078/swagger](http://localhost:5078/swagger)
- หน้าแรก `/` จะ redirect ไป Swagger อัตโนมัติ

API listen ที่ `0.0.0.0:5078` เพื่อให้เครื่องอื่นใน LAN (โทรศัพท์ / ESP32) เรียกได้ — เปิดในเบราว์เซอร์บนเครื่องเดียวกันยังใช้ `localhost` ได้เหมือนเดิม

ทิ้งหน้าต่างนี้เปิดไว้ขณะใช้งาน Flutter หรือ ESP32  
หยุด API: กด `Ctrl+C` ในหน้าต่างที่รัน `dotnet run`

---

## 2. ทดสอบ API ด้วย Swagger

เมื่อ Swagger เปิดแล้ว ลองตามลำดับนี้:

1. ขยาย `GET /api/temperature/test` → **Try it out** → **Execute**  
   จะแทรกค่าทดสอบลงฐานข้อมูล แล้วคืนแถวที่สร้าง (HTTP 200)
2. ขยาย `GET /api/temperature/latest` → **Execute**  
   ต้องเห็นแถวที่เพิ่งสร้าง
3. ขยาย `GET /api/temperature` → **Execute**  
   ประวัติทั้งหมด ใหม่สุดก่อน
4. (ทางเลือก) `POST /api/temperature` ส่งค่าเอง:

```json
{
  "temperature": 29.5,
  "humidity": 65
}
```

คาดหวัง HTTP **201**

`GET /api/temperature/test2` คืนค่าจำลองอย่างเดียว **ไม่แตะฐานข้อมูล** — ใช้เช็กว่า API ขึ้น โดยไม่สร้างแถวใหม่

---

## 3. รันแอป Flutter

ต้องรัน API ตามข้อ 1 ไว้ก่อน (`dotnet run` ที่พอร์ต 5078)

เปิด PowerShell **หน้าต่างใหม่** (อย่าปิดหน้าต่าง API):

```powershell
cd tempwatch_app
flutter pub get
```

ตั้ง Base URL ที่ `tempwatch_app/lib/api_config.dart` — เปลี่ยนบรรทัดเดียวตามเป้าทดสอบ:

| เป้าทดสอบ | `ApiConfig.baseUrl` | หมายเหตุ |
| --- | --- | --- |
| Flutter Windows | `http://localhost:5078` | ค่าเริ่มต้นเมื่อรันบนเครื่องเดียวกับ API |
| Android Emulator | `http://10.0.2.2:5078` | `10.0.2.2` คือ localhost ของเครื่อง host — emulator มอง `localhost` เป็นตัวเอง |
| โทรศัพท์จริงใน LAN | `http://<LAN-IP-ของ-PC>:5078` | เช่น `http://192.168.1.10:5078` |

ดูอุปกรณ์ที่ต่ออยู่:

```powershell
flutter devices
```

รันบน Windows:

```powershell
flutter run -d windows
```

หรือเลือกอุปกรณ์ Android:

```powershell
flutter run -d <id>
```

ในแอป:

- **Dashboard** — อุณหภูมิ / ความชื้น / เวลาล่าสุด แล้วกด **Refresh**
- **History** — รายการประวัติ ใหม่สุดบนสุด แล้วกด **Refresh**

เมื่อใช้โทรศัพท์จริง: ให้อยู่ Wi-Fi เดียวกันกับ PC (ถ้าติด timeout / connection refused ค่อยเปิด inbound TCP 5078 ใน Windows Firewall)

---

## 4. ต่อฮาร์ดแวร์ ESP32 + DHT22

ESP32 อ่าน DHT22 แล้ว POST JSON เข้า API เดิม ไม่ต้องแก้ Controller / Service / DTO

สเก็ตช์อยู่ที่ `firmware/tempwatch_esp32/`

1. คัดลอก `secrets.h.example` เป็น `secrets.h` แล้วใส่ SSID, รหัส Wi-Fi, และ **LAN IP ของ PC**  
   ห้ามใช้ `localhost` เพราะบน ESP32 แปลว่าตัวบอร์ดเอง
2. เปิด `tempwatch_esp32.ino` ใน Arduino IDE แล้วอัปโหลด
3. ดู Serial Monitor ที่ **115200** — คาดหวัง HTTP **201** ทุก 10 วินาที
4. กลับไป Swagger หรือแอป Flutter กด **Refresh** จะเห็นแถวใหม่

สายไฟสั้นๆ: DHT22 `DATA` → ESP32 **GPIO4**, `VCC` → `3V3`, `GND` → `GND`

รายละเอียดสายไฟ / ไลบรารี / แก้ปัญหาเครือข่าย / ลำดับทดสอบ 6 ขั้น อยู่ใน [`firmware/README.md`](firmware/README.md)

ESP32 กับ Flutter เป็นคนละ client: แอปบนเครื่องเดียวกับ API ยังชี้ `localhost` ได้ ส่วนบอร์ดต้องชี้ IP ใน LAN

---

## API อ้างอิง

| Method | Path | ความหมาย |
| --- | --- | --- |
| `POST` | `/api/temperature` | บันทึกค่าอุณหภูมิ / ความชื้น (HTTP 201) |
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

`RecordedAt` ตั้งโดย backend ไม่ใช่ client / ESP32

---

## แก้ปัญหาที่พบบ่อย

| อาการ | สิ่งที่เช็ก |
| --- | --- |
| Flutter / ESP32 ต่อ API ไม่ได้ | API ยังรันอยู่หรือไม่ (`dotnet run`) |
| Android Emulator ใช้ `localhost` ไม่ได้ | ต้องเป็น `http://10.0.2.2:5078` |
| โทรศัพท์ / ESP32 ใช้ `localhost` ไม่ได้ | ต้องเป็น LAN IP ของ PC เช่น `192.168.1.10` — ดูด้วย `ipconfig` |
| timeout / connection refused ทั้งที่ IP ถูก | PC กับอุปกรณ์อยู่ Wi-Fi เดียวกันหรือยัง แล้วค่อยเปิด Windows Firewall inbound TCP **5078** |
| ESP32 อ่าน DHT22 ได้ `NaN` | สายผิด, constructor เป็น `DHT11`, หรืออ่านถี่เกินไป — ดู [`firmware/README.md`](firmware/README.md) |
