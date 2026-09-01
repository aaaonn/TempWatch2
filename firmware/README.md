# Firmware TempWatch (ESP32 + DHT22)

เฟิร์มแวร์ฝั่ง embedded: อ่าน DHT22 แล้ว `POST` JSON ไป ASP.NET Core ที่มีอยู่แล้ว  
ไม่แก้ Controller / Service / DTO / Flutter architecture

```
DHT22 → ESP32 → HTTP POST JSON → ASP.NET Core → EF Core → SQL Server → Flutter (GET)
```

## สายไฟ (โมดูล DHT22 3 ขา ที่มี pull-up ในบอร์ดแล้ว)

| DHT22 | ESP32 |
| --- | --- |
| `VCC` | `3V3` |
| `DATA` | **GPIO4** |
| `GND` | `GND` |

ใช้ **3.3 V ไม่ใช่ 5 V** กับ ESP32

## ไลบรารีใน Arduino IDE

1. ติดตั้งบอร์ด **esp32** (Boards Manager → `esp32` by Espressif)
2. Library Manager:
   - **DHT sensor library** (Adafruit)
   - **Adafruit Unified Sensor** (ต้องมีคู่กัน)

ในสเก็ตช์ต้องเป็น:

```cpp
DHT dht(DHT_PIN, DHT22);
```

อย่าใส่ `DHT11` — โปรโตคอลและ timing คนละแบบ จะอ่านได้แต่ `NaN`

`WiFi.h` และ `HTTPClient.h` มากับ ESP32 core ไม่ต้องติดตั้งแยก

Arduino core **3.x**: `HTTPClient::begin` ต้องส่ง `WiFiClient` ด้วย (สเก็ตช์นี้ทำไว้แล้ว)

## ตั้งค่า Wi-Fi และ IP ของ API

รหัสและ IP เครื่องเราห้ามเข้า git

```powershell
cd firmware/tempwatch_esp32
copy secrets.h.example secrets.h
```

แก้ใน `secrets.h`:

- `WIFI_SSID` / `WIFI_PASSWORD` — Wi-Fi **2.4 GHz** (ESP32 คลาสสิกไม่มี 5 GHz)
- `API_HOST` — IPv4 ของ PC ที่รัน `dotnet run` เช่น `192.168.1.10`
- `API_PORT` — `5078`

เปิดสเก็ตช์ `tempwatch_esp32.ino` ใน Arduino IDE แล้วอัปโหลด

## ทำไมต้องเป็น LAN IP ไม่ใช่ localhost

ESP32 เป็นเครื่องคนละเครื่องกับ PC

- `localhost` / `127.0.0.1` บน ESP32 = ตัว ESP32 เอง ไม่ใช่ ASP.NET
- ต้องใส่ IP ของ PC ใน Wi-Fi บ้าน เช่น `http://192.168.1.10:5078/api/temperature`
- หลักการเดียวกับโทรศัพท์จริงใน Phase 4

ดู IP บน Windows:

```powershell
ipconfig
```

แล้วอ่าน `IPv4 Address` ของอะแดปเตอร์ Wi-Fi

เงื่อนไขเครือข่าย:

- PC กับ ESP32 อยู่ Wi-Fi **เดียวกัน** (อย่าใช้ guest network ที่แยก client)
- API ใช้ **HTTP** ไม่ใช่ HTTPS (ใบรับรอง dev ของ Kestrel จะทำให้อุปกรณ์ล้มเหลว)
- ESP32 ไม่เกี่ยวกับ CORS (CORS เป็นเรื่องเบราว์เซอร์)

ถ้า POST timeout / connection refused ทั้งที่ IP ถูกและ API รันอยู่ ค่อยเช็ค Windows Firewall inbound TCP **5078**

## ช่วงเวลาส่งค่า

DHT22 อ่านซ้ำได้ไม่ถี่กว่า **~2 วินาที** — สเก็ตช์ส่งทุก **10 วินาที**

ถี่กว่านี้จะอ่าน `NaN`, ถล่ม SQL ด้วยแถวซ้ำ, และกิน Wi-Fi โดยไม่จำเป็น

## ลำดับทดสอบ 6 ขั้น

สเก็ตช์ใน repo เป็นไฟล์เดียวที่ทำงานครบ (อ่าน → Wi-Fi → POST ค่าจริง)  
ตอนเรียน เดินตามนี้และดู Serial Monitor ที่ **115200**

### 1. อ่าน DHT22 แล้วพิมพ์ Serial

เป่าลมที่เซ็นเซอร์ อุณหภูมิ/ความชื้นใน Serial ต้องขยับ  
ถ้าได้ `NaN`: สายผิด, constructor เป็น `DHT11`, หรืออ่านถี่เกินไป — ยังไม่ต้องมีเน็ตในขั้นนี้

### 2. ต่อ Wi-Fi

Serial ขึ้น `Wi-Fi connected` และ IP ของ ESP32 อยู่ใน subnet เดียวกับ PC  
ESP32 เป็น Wi-Fi **station** (client) ไม่ใช่ access point

### 3. พิสูจน์ HTTP POST บน LAN

รัน API (`dotnet run`) แล้วรอให้สเก็ตช์ POST  
คาดหวัง **201** จาก `CreatedAtAction`  
Swagger `GET /api/temperature/latest` ต้องเห็นแถวใหม่

ถ้า timeout / connection refused = เช็ค IP ก่อน แล้วว่า API รันอยู่หรือยัง (ถ้าติดทั้งสองอย่างแล้วยังไม่ได้ ค่อยเช็ค firewall)

### 4. ส่งค่าเซ็นเซอร์จริงเป็นช่วง

ทุก 10 วินาที: อ่าน DHT22 → POST JSON `{ "temperature": ..., "humidity": ... }`

error handling แบบง่าย:

- DHT ล้มเหลว / `NaN` → ไม่ส่ง (กันค่าขยะเข้า DB)
- Wi-Fi หลุด → `WiFi.reconnect()` แล้วข้าม POST รอบนั้น
- HTTP ไม่ใช่ 2xx → พิมพ์ status แล้วรอรอบถัดไป

### 5. ตรวจ SQL Server

ไม่เขียนโค้ดใหม่

- Swagger / Postman: `GET /api/temperature/latest` และ `GET /api/temperature`
- หรือดูตาราง `TemperatureReadings` ตาม `TempWatch2/Migrations/DATABASE.md`
- `RecordedAt` เป็นเวลาที่ **backend** ตั้ง ไม่ใช่เวลาจาก ESP32

ฮาร์ดแวร์ไม่คุยกับ SQL โดยตรง — คุยกับ ASP.NET เท่านั้น (มือถือ/Postman ใช้ทางเดียวกัน)

### 6. ตรวจ Flutter

รัน API + ให้ ESP32 ส่งค่าอยู่

- Dashboard → **Refresh** เห็นอุณหภูมิ/ความชื้นล่าสุด
- History → **Refresh** มีแถวใหม่บนสุด

Flutter บนเครื่องเดียวกับ API ยังใช้ `localhost` ได้ — ESP32 กับ Flutter คนละ client คนละ URL

## แบ่งชั้นระบบ

| ชั้น | ทำอะไร | ไม่ทำอะไร |
| --- | --- | --- |
| **embedded** (โฟลเดอร์นี้) | อ่านเซ็นเซอร์, ต่อ Wi-Fi, เป็น HTTP **client** ส่ง JSON | ไม่เก็บ DB, ไม่คำนวณ `RecordedAt`, ไม่เป็น HTTP server |
| **backend / software** | รับ JSON, validation, EF Core, SQL Server, ให้ Flutter อ่านด้วย GET | ไม่ได้อ่านขา GPIO |

เทียบสั้นๆ: `loop()` วนเองไม่ใช่ Gin; `HTTPClient.POST` ≈ `http.Post` / Postman; JSON string คือ body เดียวกับ DTO เดิม
