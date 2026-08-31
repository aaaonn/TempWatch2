// Dart model จาก JSON ของ ASP.NET Core (camelCase ตาม default serializer)
// เทียบ Go: struct + json tags; factory fromJson ≈ json.Unmarshal
class TemperatureReading {
  final int id;
  final double temperature;
  final double humidity;
  final DateTime recordedAt;

  TemperatureReading({
    required this.id,
    required this.temperature,
    required this.humidity,
    required this.recordedAt,
  });

  factory TemperatureReading.fromJson(Map<String, dynamic> json) {
    return TemperatureReading(
      id: json['id'] as int,
      // num เพราะ JSON อาจเป็น 29.5 (double) หรือ 30 (int)
      temperature: (json['temperature'] as num).toDouble(),
      humidity: (json['humidity'] as num).toDouble(),
      recordedAt: DateTime.parse(json['recordedAt'] as String),
    );
  }

  // แสดงเวลาอ่านง่าย โดยไม่ต้องพึ่งแพ็กเกจ intl
  String get recordedAtLabel {
    String two(int n) => n.toString().padLeft(2, '0');
    final d = recordedAt;
    return '${d.year}-${two(d.month)}-${two(d.day)} '
        '${two(d.hour)}:${two(d.minute)}:${two(d.second)}';
  }
}
