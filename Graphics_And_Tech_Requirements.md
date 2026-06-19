# Kế Hoạch Đồ Họa & Công Nghệ Thực Thi Phục Dựng Tây Du Ký Mobile (J2ME)

Để dự án phục dựng đi vào thực tế nhanh nhất, tài liệu này hướng dẫn cách **khôi phục/tái tạo đồ họa (graphics/art assets)** và chọn lựa **công cụ/phần mềm kỹ thuật** phù hợp.

---

## 1. Phương Án Khôi Phục & Tái Tạo Đồ Họa (Asset Recovery & Art Pipeline)

Đồ họa của game gốc là định dạng 2D Pixel Art dạng lưới (Tilemap & Sprite Sheet). Chúng ta có 3 cách để thu thập và tái tạo bộ đồ họa này:

### Phương án A: Bung gói (Decompile) file game Java gốc (.jar) - [ĐÃ CHỌN]
*   **Cách thực hiện:**
    1.  Tìm và tải file game `Tây Du Ký Mobile` định dạng `.jar` hoặc `.jad` từ các kho lưu trữ game cổ (như phoneky.com, các blog game Java Việt Nam).
    2.  Đổi đuôi file `.jar` thành `.zip` và dùng WinRAR/7-Zip để giải nén.
    3.  Truy cập thư mục hình ảnh bên trong (thường tên là `/res`, `/img`, `/assets`).
*   **Kết quả đạt được:** Bạn sẽ lấy được 100% hình ảnh nguyên bản (PNG/GIF) của:
    *   Các tấm lưới gạch (Tilemap) của bản đồ Dao Trì, Nam Thiên Môn, Trường An.
    *   Các dải ảnh hoạt động (Sprite Sheets) của thú cưỡi (Bạch Hổ, Kỳ Lân), Pet (Thỏ Ngọc, Tiểu Toàn Phong) và hoạt ảnh kỹ năng.
    *   Các tệp âm thanh hiệu ứng (WAV/MIDI).

### Phương án B: Sử dụng AI vẽ lại đồ họa Pixel (Generative AI for Pixel Art)
*   **Cách thực hiện:**
    *   Sử dụng các mô hình AI như **Stable Diffusion** (với Lora chuyên biệt về Pixel Art) hoặc **Midjourney** (sử dụng prompt `--style raw` kết hợp từ khóa `pixel art, 16-bit, game asset, isometric view`).
    *   Nạp hình ảnh gốc (4 ảnh chụp của Khách hàng) làm ảnh tham khảo (Image-to-Image) để AI hiểu phong cách vẽ.
*   **Tác dụng:** Tự động tạo ra các biến thể thú cưỡi mới, pet mới hoặc chi tiết bản đồ sắc nét hơn dựa trên phong cách cũ nhưng ở độ phân giải cao hơn.

### Phương án C: Thiết kế vẽ thủ công bằng Công cụ chuyên dụng
*   **Phần mềm khuyên dùng:** **Aseprite** (Phần mềm vẽ Pixel Art và làm hoạt ảnh GIF chuyên nghiệp nhất hiện nay).
*   **Công việc:** Thuê họa sĩ vẽ lại (Redraw) dựa trên các ảnh chụp hoặc ảnh bung từ file `.jar` để làm mịn các góc răng cưa, tạo các chuyển động mượt mà hơn (tăng số khung hình animation từ 3 khung hình/giây lên 12 khung hình/giây để nhân vật chạy uyển chuyển hơn).

---

## 2. Công Nghệ & Công Cụ Lập Trình Thực Thi (Tech Stack & Tooling)

Dưới đây là bộ công cụ hoàn chỉnh để bắt đầu lập trình phục dựng:

### A. Game Client Engine (Bộ công cụ làm Game phía Client)
1.  **Unity (C#) - [ĐÃ CHỌN]:**
    *   *Ngôn ngữ:* C#.
    *   *Ưu điểm:* Hệ thống làm game 2D rất mạnh mẽ, cộng đồng hỗ trợ lớn, dễ dàng phát triển ứng dụng và xuất bản trực tiếp lên Google Play Store / Apple App Store.
2.  **Cocos Creator (Web/H5):**
    *   *Ngôn ngữ:* TypeScript / JavaScript.
    *   *Ưu điểm:* Cực kỳ nhẹ, tối ưu cho game chạy trên trình duyệt Web hoặc Zalo Mini App.

### B. Game Server & Database (Hệ thống Backend)
*   **Golang (Go) - [ĐÃ CHỌN]:** Xây dựng lõi máy chủ kết nối thông qua TCP socket/WebSockets để đảm bảo hiệu năng và xử lý đồng thời cực tốt.
*   **PostgreSQL & Redis - [ĐÃ CHỌN]:** PostgreSQL lưu trữ thông tin vĩnh viễn (Nhân vật, Đồ đạc, Pet), Redis lưu trữ tọa độ thời gian thực của người chơi để đồng bộ hóa bản đồ.

### C. Công cụ Thiết kế & Hỗ trợ Phát triển (Development Tools)
*   **Tiled Map Editor:** Phần mềm miễn phí thiết kế bản đồ dạng ô lưới. Người vẽ map sẽ xuất ra file `.tmx` hoặc `.json`, lập trình viên nạp file này vào Cocos/Unity và Server Go để tính toán vật cản (Collision).
*   **Protobuf compiler (protoc):** Biên dịch các file định nghĩa gói tin `.proto` sang code C# (cho Client) và code Go (cho Server) để đảm bảo đồng bộ định dạng truyền tin nhị phân.
*   **Git / GitHub:** Quản lý mã nguồn dự án, làm việc nhóm giữa họa sĩ vẽ pixel và lập trình viên.
