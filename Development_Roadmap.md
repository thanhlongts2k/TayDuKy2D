# Lộ trình Phát triển Dự án Phục dựng Tây Du Ký Mobile (J2ME)

Lộ trình phát triển dự án game Tây Du Ký Mobile tái sinh trên các nền tảng hiện đại (Web, Android, iOS) được chia thành 5 giai đoạn chính nhằm đảm bảo chất lượng và khả năng mở rộng.

---

## Giai đoạn 1: Nghiên cứu & Thiết kế Nguyên mẫu (MVP - Minimum Viable Product)
*   **Thời gian dự kiến:** 2 - 3 tuần.
*   **Mục tiêu:** Xây dựng khung game cơ bản nhất để kiểm thử kết nối và di chuyển.
*   **Công việc trọng tâm:**
    *   Thiết kế khung giao diện màn hình dọc (Portrait Canvas) trên Unity/Cocos Creator.
    *   Tái dựng đồ họa Pixel các nhân vật ban đầu và 3 tọa kỵ cơ bản (Bạch Hổ, Voi Trắng, Hổ Thường).
    *   Lập trình hệ thống di chuyển dạng lưới (Tilemap Navigation) trên Client.
    *   Xây dựng Server Golang tối giản hỗ trợ kết nối WebSocket, đồng bộ hóa vị trí của nhiều người chơi cùng lúc trên một map nhỏ (ví dụ: Dao Trì).
    *   *Kết quả:* Người chơi có thể đăng nhập, di chuyển cùng nhau trên map và chat được với nhau.

---

## Giai đoạn 2: Phát triển Tính năng Cốt lõi (Alpha Stage)
*   **Thời gian dự kiến:** 4 - 6 tuần.
*   **Mục tiêu:** Hoàn thiện cơ chế nhập vai, nhiệm vụ và hệ thống môn phái.
*   **Công việc trọng tâm:**
    *   Tích hợp hệ thống Nhiệm vụ (Quest System) sơ cấp từ các NPC Đồng Tử, Không Tiên và chuỗi nhiệm vụ giải cứu Tôn Ngộ Không tại Dao Trì.
    *   Lập trình hệ thống Môn phái (Giáo phái) và cơ chế học kỹ năng đặc trưng.
    *   Hoàn thiện hệ thống Thú cưỡi (Mounts) và Tiên sủng (Pets), bao gồm cơ chế cộng dồn chỉ số thuộc tính khi cưỡi/triệu hồi.
    *   Tích hợp hệ thống cơ sở dữ liệu PostgreSQL để lưu trữ thông tin nhân vật, cấp độ và trang bị.
    *   *Kết quả:* Người chơi có thể làm nhiệm vụ, lên cấp, học kỹ năng, cưỡi thú và lưu trữ dữ liệu nhân vật khi thoát game.

---

## Giai đoạn 3: Hệ thống Chiến đấu & Hoạt động Cộng đồng (Beta Stage)
*   **Thời gian dự kiến:** 4 - 5 tuần.
*   **Mục tiêu:** Đưa hệ thống chiến đấu theo lượt và các tính năng tương tác cộng đồng vào hoạt động.
*   **Công việc trọng tâm:**
    *   Lập trình màn hình chiến đấu theo lượt (Turn-based Combat Engine) với các lệnh đánh, dùng phép, dùng bình thuốc và Auto.
    *   Xây dựng hệ thống quái vật dạo bản đồ (như Tiểu Toàn Phong) và phụ bản ải (81 Kiếp nạn).
    *   Tính năng Bang hội (Guild) và hệ thống chuyển kênh (Khu Phái).
    *   Hệ thống Giao dịch giữa người chơi với nhau và Shop Kỳ Trân Các.
    *   *Kết quả:* Hoàn thiện vòng lặp chơi game (Game Loop): Làm nhiệm vụ -> Đánh quái lên cấp -> Tìm trang bị/Pháp bảo -> Nâng cấp thú cưỡi -> Tham gia Bang hội.

---

## Giai đoạn 4: Tối ưu hóa, Kiểm thử Tải & Bảo mật
*   **Thời gian dự kiến:** 2 - 3 tuần.
*   **Mục tiêu:** Đảm bảo game chạy mượt mà, không giật lag và ngăn chặn gian lận (Hack/Cheat).
*   **Công việc trọng tâm:**
    *   Kiểm tra tính chịu tải của Server Golang (Stress Test) mô phỏng 5,000 - 10,000 người chơi cùng lúc.
    *   Tối ưu hóa tài nguyên đồ họa (Sprite Atlases, Compression) để bản chơi trên Web/Mobile tải cực nhanh dưới 10 giây.
    *   Xây dựng hệ thống chống hack di chuyển, hack sát thương bằng cách tính toán và xác thực toàn bộ hành động di chuyển/gây sát thương ở phía Server.
    *   Mở thử nghiệm giới hạn (Closed Beta) cho một nhóm người chơi nhỏ để thu thập phản hồi và sửa lỗi.

---

## Giai đoạn 5: Phát hành & Vận hành (Go-Live)
*   **Mục tiêu:** Ra mắt game chính thức và duy trì phát triển cộng đồng.
*   **Công việc trọng tâm:**
    *   Đóng gói game chạy Web (chơi trực tiếp trên cổng game).
    *   Xuất bản file `.apk`/`.ipa` lên Google Play Store và Apple App Store.
    *   Lên kế hoạch cập nhật nội dung định kỳ (kiếp nạn mới, tọa kỵ huyền thoại mới như Hỏa Phượng Hoàng, sự kiện đua top, bang chiến liên server).
