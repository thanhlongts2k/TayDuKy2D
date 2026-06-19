# Tài liệu Nguồn Tham Khảo & Bàn giao Dự án Tây Du Ký Mobile (J2ME)

Tài liệu này lưu trữ các nguồn thông tin lịch sử, từ khóa tìm kiếm và cơ sở dữ liệu đã tham khảo để xây dựng kịch bản phục dựng game. Các Agent khác sau khi tiếp quản dự án có thể dựa vào đây để tiếp tục nghiên cứu sâu hơn.

---

## 1. Thông tin Tổng quan về Game gốc
*   **Tên chính thức:** Tây Du Ký Mobile (hoặc Tây Du Mobile).
*   **Nhà phát hành (NPH):** ME Corp (Mobile Entertainment Corporation).
*   **Thời gian vận hành:** 2013 - 2015.
*   **Nền tảng hỗ trợ:** Java (J2ME - file `.jar`/`.jad`) và Android đời đầu.
*   **Đặc trưng đồ họa:** 2D Pixel Art, phong cách hoạt hình, tỷ lệ khung hình dọc (Portrait UI) tối ưu cho độ phân giải màn hình dòng máy Nokia cổ điển (240x320, 320x240...).

---

## 2. Các Nguồn Tư liệu Tìm kiếm & Tham khảo chính
Do tựa game đã đóng cửa lâu năm, các thông tin chi tiết được tái dựng thông qua việc tổng hợp từ các bài báo game, diễn đàn hoài cổ và cơ sở dữ liệu internet:

### A. Các trang tin tức và báo game cũ:
1.  **GameK.vn:**
    *   Các bài viết giới thiệu thời điểm ra mắt game (Tháng 10/2013), sự kiện NPH ME Corp mời diễn viên Lục Tiểu Linh Đồng sang Việt Nam quảng bá game.
    *   Mô tả hệ thống chiến đấu turn-based, sự xuất hiện của hơn 700 loại Tiên Sủng chia làm 3 phân cấp: *Trân Thú, Tán Tiên, Kim Tiên*.
2.  **Dzogame.vn / Xaluannews.com:**
    *   Các bài viết giới thiệu tính năng phụ bản đặc sắc: Hái Trộm Đào Tiên (Đào viên), Đại Náo Thiên Cung, Đại Náo Thủy Cung.
    *   Phân tích hệ thống Tọa kỵ (Thú cưỡi): Tiên Hạc, Phượng Hoàng, Xích Viêm Thú, Bạch Hổ...
3.  **Blogspot & Diễn đàn Java cổ (như ya4r.net, m4v.me):**
    *   Các tệp lưu trữ bài viết hướng dẫn (guides) của người chơi xưa về cách phân bổ điểm thuộc tính nhân vật, cách tẩy tủy Tiên sủng và cách vượt ải các kiếp nạn.

### B. Các từ khóa tìm kiếm hiệu quả để tra cứu thêm:
*   `"Tây Du Ký" "ME Corp" OR "Mcorp" java game` (Tra cứu lịch sử game Java).
*   `"Tây Du Ký" "ME Corp" "Tiên sủng" OR "Pet" kỹ năng` (Tra cứu hệ thống Pet).
*   `"Tây Du Ký" "ME Corp" "tọa kỵ" OR "thú cưỡi"` (Tra cứu hệ thống thú cưỡi).
*   `"Tây Du Ký" "ME Corp" "Trường An" OR "Hội Bàn Đào" OR "Dao Trì"` (Tra cứu tuyến bản đồ).

---

## 3. Các điểm mấu chốt đã thống nhất từ Tư liệu Ảnh của Khách hàng
Khi tiếp nhận dự án, các Agent cần lưu ý các phân cảnh thực tế thu thập được từ ảnh chụp của Khách hàng:
1.  **Ảnh 1 (Cấp 33):**
    *   Nhân vật đứng ở map làng quê, đang cưỡi thú cưỡi **Bạch Hổ** (phía trên có người chơi `Na no boy`).
    *   NPC xuất hiện: **Đồng Tử**, **Không Tiên**.
    *   Quái vật: **Tiểu Toàn Phong**.
2.  **Ảnh 2 (Cấp 9):**
    *   Phân cảnh tại **Dao Trì** (Thiên Đình).
    *   Ba nhân vật cốt truyện bị trói chịu tội phạt: **Quyển Liêm**, **Tề Thiên Đại Thánh**, **Thiên Bồng**.
    *   Thông báo hệ thống: *"Bạn chưa vào Giáo"* (yêu cầu người chơi đạt cấp độ 10 để gia nhập một trong 3 tộc: **Thần, Ma, Yêu**).
3.  **Ảnh 3 (Cấp 32) & Ảnh 4 (Cấp 32):**
    *   Phân cảnh tại **Nam Thiên Môn / Thiên Đình** (các đảo cỏ xanh lơ lửng trên mây trắng).
    *   Xuất hiện loạt thú cưỡi cao cấp: **Lục Nha Bạch Tượng** (Voi trắng), **Thanh Sư** (Sư tử), **Tiên Hạc**, **Hỏa Kỳ Lân** (nhân vật đang cưỡi), **Hỏa Phượng Hoàng** (đang bay trên trời), và **Lục Bảo Phi Kiếm** (ngự kiếm phi hành phát sáng xanh lục).
    *   Dòng thông báo hệ thống nhắc đến bản đồ **Sau Hoa Quả Sơn** (khai thác Tinh Kho) và cảnh giới tu tiên **Huyền Tiên Bậc 1**.
