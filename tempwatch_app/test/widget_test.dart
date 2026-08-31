import 'package:flutter_test/flutter_test.dart';
import 'package:tempwatch_app/main.dart';

void main() {
  testWidgets('แอปมีแท็บ Dashboard และ History', (WidgetTester tester) async {
    await tester.pumpWidget(const TempWatchApp());

    expect(find.text('Dashboard'), findsWidgets);
    expect(find.text('History'), findsOneWidget);
  });
}
