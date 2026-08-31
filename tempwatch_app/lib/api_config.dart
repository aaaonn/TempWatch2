// URL ของ ASP.NET Core — เก็บที่เดียวแล้วสลับตามเป้าทดสอบ
// เทียบ Go: ค่าคงที่ base URL ก่อน http.Get ไม่ทำ flavor / dart-define ในรอบนี้
class ApiConfig {
  // ค่าเริ่มต้น: Flutter Windows เรียก API บนเครื่องเดียวกัน
  //
  // สลับบรรทัดนี้เมื่อทดสอบเป้าอื่น:
  // - Android Emulator: 'http://10.0.2.2:5078'
  //   (10.0.2.2 = localhost ของเครื่อง host — emulator มอง localhost เป็นตัวเอง)
  // - โทรศัพท์จริงใน LAN: 'http://<LAN-IP-ของ-PC>:5078'
  //   เช่น 'http://192.168.1.10:5078' แล้วเปิด Windows Firewall พอร์ต 5078
  // static const String baseUrl = 'http://localhost:5078';
  static const String baseUrl = 'http://10.0.2.2:5078'; // Android Emulator
}
