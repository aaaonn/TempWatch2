# TempWatch Database Schema

สรุป schema ของฐานข้อมูลจาก EF Core migration ปัจจุบัน

## Database

| รายการ | ค่า |
| --- | --- |
| Database name | `TempWatch` |
| Provider | SQL Server (LocalDB) |
| DbContext | `TempWatchDbContext` |
| Connection string key | `ConnectionStrings:TempWatch` |

## Tables

### TemperatureReadings

ตารางเก็บค่าอุณหภูมิและความชื้นที่บันทึกได้จากระบบ

| Column | SQL type | Nullable | Notes |
| --- | --- | --- | --- |
| Id | int | no | Primary key, Identity (1, 1) |
| Temperature | float | no | ค่าอุณหภูมิ |
| Humidity | float | no | ค่าความชื้น |
| RecordedAt | datetime2 | no | เวลาที่บันทึก |

**Primary key:** `PK_TemperatureReadings` (`Id`)

## Migrations

| Migration | Description |
| --- | --- |
| `20260827051044_InitialCreate` | สร้างตาราง `TemperatureReadings` |
