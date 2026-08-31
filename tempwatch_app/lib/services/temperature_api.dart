import 'dart:convert';

import 'package:http/http.dart' as http;

import '../api_config.dart';
import '../models/temperature_reading.dart';

// ชั้นเรียก HTTP — ไม่ใช่ business layer ของ C#
// เทียบ Go: http.Get + json.Unmarshal ไม่ใช่ service ที่คุย GORM
class TemperatureApi {
  static const _timeout = Duration(seconds: 10);

  Future<TemperatureReading?> getLatest() async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/api/temperature/latest');
    final response = await http.get(uri).timeout(_timeout);

    if (response.statusCode == 200) {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      return TemperatureReading.fromJson(map);
    }

    // ตารางว่าง API ส่ง 404 — ถือว่า empty ไม่ใช่ error ทั่วไป
    // (ต่างจาก GET /api/temperature ที่คืน 200 + [])
    if (response.statusCode == 404) {
      return null;
    }

    throw Exception('โหลดค่าล่าสุดไม่สำเร็จ (${response.statusCode})');
  }

  Future<List<TemperatureReading>> getHistory() async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/api/temperature');
    final response = await http.get(uri).timeout(_timeout);

    if (response.statusCode == 200) {
      final list = jsonDecode(response.body) as List<dynamic>;
      return list
          .map(
            (item) =>
                TemperatureReading.fromJson(item as Map<String, dynamic>),
          )
          .toList();
    }

    throw Exception('โหลดประวัติไม่สำเร็จ (${response.statusCode})');
  }
}
