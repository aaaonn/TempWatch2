import 'package:flutter/material.dart';

import '../models/temperature_reading.dart';
import '../services/temperature_api.dart';

// หน้า Dashboard: อุณหภูมิ / ความชื้น / เวลาล่าสุด
// สถานะใน StatefulWidget: loading / empty / error / data
// setState = บอก Flutter ว่าข้อมูลเปลี่ยนแล้วให้วาดใหม่ (ไม่มีใน Go API)
class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  final TemperatureApi _api = TemperatureApi();

  bool _loading = true;
  String? _error;
  TemperatureReading? _reading;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final latest = await _api.getLatest();
      if (!mounted) return;
      setState(() {
        _reading = latest;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.toString();
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Dashboard')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Expanded(child: Center(child: _body())),
            FilledButton.icon(
              onPressed: _loading ? null : _load,
              icon: const Icon(Icons.refresh),
              label: const Text('Refresh'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _body() {
    if (_loading) {
      return const CircularProgressIndicator();
    }

    if (_error != null) {
      return _message(
        icon: Icons.error_outline,
        text: 'โหลดข้อมูลไม่สำเร็จ\n$_error',
      );
    }

    if (_reading == null) {
      return _message(
        icon: Icons.inbox_outlined,
        text: 'ยังไม่มีข้อมูล\nลอง POST จาก Postman / Swagger แล้วกด Refresh',
      );
    }

    final reading = _reading!;
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Text(
          '${reading.temperature.toStringAsFixed(1)} °C',
          style: Theme.of(context).textTheme.displaySmall,
        ),
        const SizedBox(height: 8),
        Text(
          'ความชื้น ${reading.humidity.toStringAsFixed(1)} %',
          style: Theme.of(context).textTheme.titleLarge,
        ),
        const SizedBox(height: 16),
        Text('บันทึกเมื่อ ${reading.recordedAtLabel}'),
      ],
    );
  }

  Widget _message({required IconData icon, required String text}) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Icon(icon, size: 48),
        const SizedBox(height: 12),
        Text(text, textAlign: TextAlign.center),
      ],
    );
  }
}
