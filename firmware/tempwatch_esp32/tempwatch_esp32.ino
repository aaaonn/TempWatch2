// TempWatch — ESP32 + DHT22 ส่งอุณหภูมิ/ความชื้นไป ASP.NET Core
//
// สเก็ตช์นี้รวม 4 ขั้นเรียนไว้ในไฟล์เดียว (อ่าน Serial → Wi-Fi → HTTP → ค่าจริง):
//   1. อ่าน DHT22 แล้วพิมพ์ Serial 115200
//   2. ต่อ Wi-Fi แบบ station (client) แล้วพิมพ์ IP ของ ESP32
//   3. HTTP POST JSON ไป POST /api/temperature (เหมือน Postman แต่บอร์ดเป็นคนยิง)
//   4. ส่งค่าเซ็นเซอร์จริงทุก 10 วินาที + ข้ามรอบเมื่ออ่านไม่ได้ / Wi-Fi หลุด
//
// เทียบกับที่คุ้น:
//   loop()            ≈ โปรแกรมที่วนเอง ไม่ใช่ HTTP server แบบ Gin
//   HTTPClient.POST   ≈ http.Post ใน Go หรือปุ่ม Send ใน Postman
//   JSON String       ≈ body ที่ Flutter/Postman ส่งเข้า DTO เดิม
//   delay(10000)      ≈ ไม่มี background worker — พอสำหรับโปรเจกต์เรียนนี้
//
// ไลบรารี: DHT sensor library (Adafruit) + Adafruit Unified Sensor
// บอร์ด: ESP32  (Arduino core 3.x — http.begin ต้องส่ง WiFiClient ด้วย)
// เซ็นเซอร์: DHT22 (อย่าใส่ DHT11 ใน constructor — โปรโตคอลคนละแบบ จะได้ NaN)

#include <WiFi.h>
#include <HTTPClient.h>
#include <DHT.h>
#include "secrets.h"

// GPIO4 = D4 บนบอร์ด ESP32 หลายรุ่น — ดูสายไฟใน firmware/README.md
#define DHT_PIN 4

// DHT22 ไม่ใช่ DHT11: timing / checksum คนละชุด ใส่ผิดแล้วอ่านได้แต่ NaN
DHT dht(DHT_PIN, DHT22);

// DHT22 อ่านซ้ำได้ไม่ถี่กว่า ~2 วินาที (datasheet)
// ถี่กว่านี้ได้ NaN, ถล่ม SQL ด้วยแถวซ้ำ, กิน Wi-Fi โดยไม่จำเป็น
const unsigned long INTERVAL_MS = 10000;

void connectWiFi() {
  Serial.print("Connecting to Wi-Fi: ");
  Serial.println(WIFI_SSID);

  // WIFI_STA = station (client) ไม่ใช่ access point
  // ESP32 คลาสสิกใช้ได้แค่ 2.4 GHz — SSID 5 GHz จะต่อไม่ติด
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  unsigned long startedAt = millis();
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
    if (millis() - startedAt > 20000) {
      Serial.println();
      Serial.println("Wi-Fi timeout — จะลองใหม่ใน loop()");
      return;
    }
  }

  Serial.println();
  Serial.print("Wi-Fi connected, IP: ");
  Serial.println(WiFi.localIP());
}

// สร้าง JSON ด้วย String ธรรมดา ไม่ใช้ ArduinoJson ในรอบนี้
// รูปเดียวกับ Postman: {"temperature":29.5,"humidity":64}
bool postReading(float temperature, float humidity) {
  // ESP32 Arduino core 3.x: begin() ต้องมี WiFiClient ไม่ใช่แค่ URL
  WiFiClient client;
  HTTPClient http;

  String url = String("http://") + API_HOST + ":" + String(API_PORT) + "/api/temperature";
  String json = "{\"temperature\":" + String(temperature, 1) +
                ",\"humidity\":" + String(humidity, 1) + "}";

  Serial.print("POST ");
  Serial.println(url);
  Serial.print("body: ");
  Serial.println(json);

  if (!http.begin(client, url)) {
    Serial.println("HTTP begin failed (ตรวจ API_HOST / พอร์ต)");
    return false;
  }

  http.addHeader("Content-Type", "application/json");
  int status = http.POST(json);
  http.end();

  Serial.print("HTTP status: ");
  Serial.println(status);

  // ASP.NET CreatedAtAction คืน 201 เมื่อบันทึกสำเร็จ
  if (status == 201) {
    Serial.println("บันทึกสำเร็จ (201 Created)");
    return true;
  }

  // ไม่ใช่ 2xx: พิมพ์รหัสแล้วรอรอบถัดไป ไม่ retry ซับซ้อน
  if (status < 200 || status > 299) {
    Serial.println("POST ไม่สำเร็จ — ถ้า timeout/connection refused ให้เช็ค IP, API รันอยู่, firewall 5078");
  }
  return false;
}

void setup() {
  Serial.begin(115200);
  delay(1000);

  Serial.println();
  Serial.println("TempWatch ESP32 + DHT22");

  dht.begin();
  connectWiFi();
}

void loop() {
  float temperature = dht.readTemperature();
  float humidity = dht.readHumidity();

  // NaN = สายผิด / constructor เป็น DHT11 / อ่านถี่เกินไป / เซ็นเซอร์ยังไม่พร้อม
  if (isnan(temperature) || isnan(humidity)) {
    Serial.println("DHT22 อ่านไม่สำเร็จ (NaN) — ข้าม POST รอบนี้ กันค่าขยะเข้า DB");
    delay(INTERVAL_MS);
    return;
  }

  Serial.print("DHT22  ");
  Serial.print(temperature, 1);
  Serial.print(" °C  ");
  Serial.print(humidity, 1);
  Serial.println(" %");

  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("Wi-Fi หลุด — reconnect แล้วข้าม POST รอบนี้");
    WiFi.reconnect();
    delay(INTERVAL_MS);
    return;
  }

  Serial.print("Wi-Fi IP: ");
  Serial.println(WiFi.localIP());

  postReading(temperature, humidity);
  Serial.println("========================================================");

  delay(INTERVAL_MS);
}
