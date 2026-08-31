import 'package:flutter/material.dart';

import '../models/temperature_reading.dart';
import '../services/temperature_api.dart';

// หน้า History: รายการอ่านง่าย เรียงใหม่สุดก่อนตาม API
// empty = list ว่าง (200 + []) ไม่ใช้ 404 แบบ latest
class HistoryScreen extends StatefulWidget {
  const HistoryScreen({super.key});

  @override
  State<HistoryScreen> createState() => _HistoryScreenState();
}

class _HistoryScreenState extends State<HistoryScreen> {
  final TemperatureApi _api = TemperatureApi();

  bool _loading = true;
  String? _error;
  List<TemperatureReading> _items = [];

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
      final history = await _api.getHistory();
      if (!mounted) return;
      setState(() {
        _items = history;
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
      appBar: AppBar(title: const Text('History')),
      body: Column(
        children: [
          Expanded(child: _body()),
          Padding(
            padding: const EdgeInsets.all(16),
            child: SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                onPressed: _loading ? null : _load,
                icon: const Icon(Icons.refresh),
                label: const Text('Refresh'),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _body() {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Text(
            'โหลดประวัติไม่สำเร็จ\n$_error',
            textAlign: TextAlign.center,
          ),
        ),
      );
    }

    if (_items.isEmpty) {
      return const Center(child: Text('ยังไม่มีประวัติ'));
    }

    return ListView.separated(
      itemCount: _items.length,
      separatorBuilder: (_, _) => const Divider(height: 1),
      itemBuilder: (context, index) {
        final item = _items[index];
        return ListTile(
          title: Text(
            '${item.temperature.toStringAsFixed(1)} °C  ·  '
            '${item.humidity.toStringAsFixed(1)} %',
          ),
          subtitle: Text(item.recordedAtLabel),
        );
      },
    );
  }
}
