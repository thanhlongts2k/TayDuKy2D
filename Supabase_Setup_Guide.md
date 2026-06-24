# Hướng Dẫn Cài Đặt Supabase cho Hệ Thống Map (Bước 5 — Sandbox/Dev)

Tài liệu này hướng dẫn **từng bước** đưa map lên PostgreSQL của Supabase, để admin có thể tùy biến map mà không cần build lại client. Dành cho người **chưa có tài khoản nào**.

> Sau khi làm xong, server đọc map từ Supabase (qua `DATABASE_URL`). Nếu không đặt `DATABASE_URL`, server tự động dùng `maps.json` như cũ — nên bạn không sợ hỏng prototype.

---

## Bước A — Tạo project Supabase (~5 phút)

1. Vào https://supabase.com → **Start your project** → đăng nhập bằng GitHub.
2. **New project**:
   - *Name:* `tayduky-sandbox`
   - *Database Password:* đặt 1 mật khẩu mạnh → **lưu lại** (sẽ dùng ở Bước C).
   - *Region:* chọn gần bạn (vd: Singapore).
3. Đợi ~2 phút cho project khởi tạo xong.

---

## Bước B — Tạo bảng `maps`

1. Mở project → menu trái **SQL Editor** → **New query**.
2. Dán toàn bộ nội dung file [`db/schema.sql`](db/schema.sql) vào và bấm **Run**.
3. Vào **Table Editor** → thấy bảng `maps` là OK.

---

## Bước C — Lấy chuỗi kết nối (DATABASE_URL)

1. **Project Settings** (bánh răng) → **Database** → mục **Connection string** → tab **URI**.
2. Copy chuỗi dạng:
   ```
   postgresql://postgres:[YOUR-PASSWORD]@db.xxxxxxxx.supabase.co:5432/postgres
   ```
   Thay `[YOUR-PASSWORD]` bằng mật khẩu đã đặt ở Bước A.
   > Gợi ý: nếu deploy thật, nên dùng **Connection Pooler** (cổng 6543) thay vì 5432.
3. Trong thư mục dự án, copy file `.env.example` thành `.env` rồi dán vào:
   ```
   DATABASE_URL=postgresql://postgres:matkhau@db.xxxx.supabase.co:5432/postgres
   MAP_RELOAD_SECONDS=30
   ```
   > File `.env` đã được `.gitignore` chặn — **không bao giờ commit mật khẩu**.

---

## Bước D — Bơm dữ liệu map hiện có lên DB

Chạy script seed (đẩy `maps.json` → bảng `maps`, idempotent):

**PowerShell:**
```powershell
python db/seed_maps.py
```
*(Script tự đọc `DATABASE_URL` từ biến môi trường. Nếu chưa export, chạy:)*
```powershell
$env:DATABASE_URL = (Get-Content .env | Select-String '^DATABASE_URL=').ToString().Split('=',2)[1]
python db/seed_maps.py
```

Kết quả mong đợi:
```
[OK] Seeded 2 map(s) into the 'maps' table: [101, 102]
```

---

## Bước E — Chạy server với backend Postgres

```powershell
python server.py
```
Log mong đợi:
```
[CONFIG] Map backend: PostgreSQL (DATABASE_URL is set).
[START] Map repository loaded 2 maps from PostgreSQL.
[START] Periodic map reload every 30s enabled.
```
→ Server giờ phục vụ map **từ Supabase**. Client không cần đổi gì (vẫn dùng packet `1006/2006`).

---

## Bước F — Admin tùy biến map (không cần build client)

1. Supabase → **Table Editor** → bảng `maps` → sửa cột `data` (JSONB) của map muốn đổi
   (ví dụ thêm/bớt phần tử trong `obstacles`, đổi `spawn_x`/`spawn_y`, thêm `npcs`/`portals`).
2. Lưu lại → trong vòng `MAP_RELOAD_SECONDS` (30s), server tự nạp lại, version map đổi.
3. Người chơi vào/đổi map lần kế tiếp sẽ nhận map mới (client tự cache lại).

> **Đây chính là mục tiêu Bước 5:** admin sửa map trên panel có sẵn của Supabase, người chơi nhận ngay, **không phát hành lại app**.

---

## Ghi chú & các bước tiếp theo

- **Ảnh nền map (CDN):** hiện `bg_resource_path` vẫn trỏ tới ảnh nhúng trong client. Bước kế tiếp sẽ
  chuyển ảnh lên **Supabase Storage** và đổi `bg_resource_path` thành URL để client tải runtime
  (có CDN tích hợp). Sẽ làm trong increment sau.
- **Bảo mật:** mới chỉ dùng cho sandbox. Khi lên production cần bật Row Level Security và tách
  quyền admin (xem mục 3.B/5 trong `Map_System_Redesign_Proposal.md`).
- **Driver:** server dùng `psycopg2` (đồng bộ) bọc trong `asyncio.to_thread`; DB chỉ bị gọi lúc
  load/refresh, không gọi trên hot-path mỗi bước đi → không chặn event loop.
