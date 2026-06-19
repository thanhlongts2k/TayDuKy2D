# Danh Mục Chi Tiết Hệ Thống: Bản Đồ, NPC, Tiên Sủng, Nhiệm Vụ & Phó Bản

Tài liệu này liệt kê chi tiết danh sách tất cả tài nguyên nội dung trong game để phục vụ lập trình và cấu hình dữ liệu tĩnh (JSON) cho dự án phục dựng Tây Du Ký Mobile.

---

## 1. Danh Sách Bản Đồ & Quái Vật (Map & Monster Directory)

| ID | Tên Bản Đồ | Cấp Độ | Bản Đồ Liên Thông | Quái Vật Spawns | Mô Tả Mỹ Thuật |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **101** | Hội Bàn Đào | 1 - 4 | Dao Trì | Yêu Hoa, Yêu Thỏ | Tiên cảnh mây mù, đào tiên xum xuê. |
| **102** | Dao Trì | 5 - 7 | Hội Bàn Đào, Nam Thiên Môn | Thiên Binh Tập Sự | Quảng trường gạch ngọc trắng, có cột phong ấn. |
| **103** | Nam Thiên Môn | 8 - 9 | Dao Trì, Trường An Thành | Thiên Binh Tuần Tra, Tiểu Toàn Phong | Đảo cỏ lửng lơ trong mây, có bậc thang đá dài. |
| **201** | Trường An Thành | Thành | Nam Thiên Môn, Cao Lão Trang | Không (Khu an toàn) | Kinh đô cổ kính nhà Đường, phố xá buôn bán sầm uất. |
| **202** | Cao Lão Trang | 10 - 15 | Trường An, Hoa Quả Sơn | Yêu Heo Con, Bọ Cánh Cứng | Làng quê thanh bình, ruộng lúa và hàng rào tre. |
| **203** | Hoa Quả Sơn | 16 - 25 | Cao Lão Trang, Đông Hải | Yêu Hầu Gậy, Yêu Hầu Đá | Rừng cây um túm, dốc đá và thác nước đổ (Thủy Liêm Động). |
| **204** | Sau Hoa Quả Sơn | Khai thác | Hoa Quả Sơn | Hầu Vệ Binh | Map khai khoáng, nhiều quặng và tinh thạch phát sáng. |
| **205** | Đông Hải Long Cung| 26 - 35 | Hoa Quả Sơn, Hỏa Diệm Sơn | Tuần Biển Dạ Soa, Tôm Binh | Thủy cung xanh lam, rạn san hô lấp lánh và bọt khí. |
| **206** | Hỏa Diệm Sơn | 36 - 45 | Đông Hải, Sư Đà Lĩnh | Quỷ Lửa, Nham Thạch Thú | Vùng đất dung nham đỏ rực, nứt nẻ bốc khói. |
| **207** | Sư Đà Lĩnh | 46 - 55 | Hỏa Diệm Sơn | Yêu Điêu Binh, Sư Đà Yêu | Hang động u ám đầy xương cốt và đầm lầy âm khí. |

---

## 2. Danh Sách NPC Chức Năng (NPC Directory)

| ID | Tên NPC | Bản Đồ Ngự Trị | Tọa Độ (Grid) | Vai Trò & Chức Năng Chính |
| :--- | :--- | :--- | :--- | :--- |
| **1001** | Đồng Tử | Hội Bàn Đào | `(12, 10)` | Hướng dẫn tân thủ, phát trang bị và dược phẩm sơ cấp. |
| **1002** | Không Tiên | Nam Thiên Môn | `(25, 18)` | Hướng dẫn gia nhập thế lực (Thần - Ma - Yêu), nâng cảnh giới. |
| **1003** | Tề Thiên Đại Thánh| Dao Trì | `(15, 15)` | NPC nhiệm vụ chính tuyến giải cứu Ngộ Không. |
| **1004** | Thiên Bồng | Dao Trì | `(8, 15)` | NPC nhiệm vụ chính tuyến của Trư Bát Giới. |
| **1005** | Quyển Liêm | Dao Trì | `(22, 15)` | NPC nhiệm vụ chính tuyến của Sa Tăng. |
| **2001** | Đường Thái Tông | Trường An Thành | `(15, 25)` | Giao chiếu thư và sứ mệnh đi Tây Thiên thỉnh kinh. |
| **2002** | Thầy Thuốc | Trường An Thành | `(5, 12)` | Bán bình HP/MP, chữa trị miễn phí dưới cấp 15. |
| **2003** | Thợ Rèn | Trường An Thành | `(8, 30)` | Bán trang bị cơ bản, cường hóa, tẩy luyện, khảm ngọc. |
| **2004** | Quản Kho | Trường An Thành | `(20, 20)` | Quản lý kho đồ dùng chung của tài khoản. |
| **2005** | Trưởng Thôn Cao | Cao Lão Trang | `(10, 10)` | Giao chuỗi nhiệm vụ giải cứu Cao Lão Trang khỏi Yêu quái mặt heo. |
| **2006** | Bồ Tát Quan Âm | Nam Thiên Môn | `(30, 30)` | Giao nhiệm vụ phó bản tuần hoàn và pháp bảo nâng cấp. |

---

## 3. Danh Sách Tiên Sủng Điển Hình (Pet Catalog)

### A. Phân lớp Trân Thú (Pet Thường - Bắt dã ngoại)
*   **Thỏ Ngọc:** Bắt tại *Hội Bàn Đào*. Kỹ năng: *Tấn công nhanh* (gây sát thương vật lý nhẹ).
*   **Yêu Hầu Con:** Bắt tại *Hoa Quả Sơn*. Kỹ năng: *Đập gậy* (tăng 10% bạo kích lượt kế).
*   **Tiểu Miêu:** Bắt tại *Cao Lão Trang*. Kỹ năng: *Cào cấu* (gây chảy máu rút máu đối thủ).

### B. Phân lớp Tán Tiên (Pet Hiếm - Phó bản / Ấp trứng)
*   **Tiểu Toàn Phong:** Bắt tại *Nam Thiên Môn*. Kỹ năng: *Cờ lệnh tuần sơn* (tăng 15% phòng thủ trong 3 lượt).
*   **Dạ Soa Tuần Biển:** Bắt tại *Đông Hải Long Cung*. Kỹ năng: *Đâm ba chĩa* (giảm 20% tốc độ đánh mục tiêu).
*   **Hỏa Linh Nhi:** Bắt tại *Hỏa Diệm Sơn*. Kỹ năng: *Cầu lửa* (sát thương phép hệ Hỏa diện rộng).

### C. Phân lớp Kim Tiên (Pet Thần Thoại - Ghép mảnh / Sự kiện)
*   **Kỳ Lân Con:** Nhận qua ấp trứng Kim Tiên. Kỹ năng: *Thụy thú ban phước* (hồi 20% HP toàn đội, tăng 10% phòng thủ).
*   **Tiểu Phượng Hoàng:** Nhận qua ghép linh hồn. Kỹ năng: *Nirvana* (tự động hồi sinh với 30% HP khi tử trận, 1 lần/trận).
*   **Ngộ Không Con:** Phần thưởng sự kiện đặc biệt. Kỹ năng: *Vạn quân phân thân* (triệu hồi ảo ảnh bạo kích diện rộng).

---

## 4. Chuỗi 12 Nhiệm Vụ Chính Tuyến Hoàn Chỉnh (Quest Directory)

| ID | Tên Nhiệm Vụ | Cấp Độ | NPC Giao | Bản Đồ | Mục Tiêu Yêu Cầu | Phần Thưởng |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **1** | Náo Loạn Đào Viên | 1 | Đồng Tử | Hội Bàn Đào | Tiêu diệt 5 Yêu Hoa | 100 EXP, 10 Vàng, 1 Gậy gỗ |
| **2** | Quả Đào Đầu Tiên | 2 | Đồng Tử | Hội Bàn Đào | Nhặt 3 Đào Tiên dâng hiến | 150 EXP, 15 Vàng |
| **3** | Oan Khuất Dao Trì | 3 | Tề Thiên Đại Thánh| Dao Trì | Đối thoại với Thiên Bồng | 200 EXP, 20 Vàng |
| **4** | Mảnh Xích Giam Cầm| 5 | Tề Thiên Đại Thánh| Dao Trì | Thu thập 5 Mảnh Xích Thần từ Thiên Binh Tập Sự | 500 EXP, 50 Vàng, 1 Nón vải |
| **5** | Khuyên Giải Sa Tăng| 7 | Quyển Liêm | Dao Trì | Thu thập 1 Chén Lưu Ly U Minh từ rương phong ấn | 800 EXP, 80 Vàng |
| **6** | Chiếu Thư Hạ Phàm | 9 | Không Tiên | Nam Thiên Môn | Tiêu diệt 3 Thiên Binh Tuần Tra để rời Thiên Đình | 1200 EXP, 100 Vàng, Chọn Faction |
| **7** | Kiến Diện Đường Vương| 10 | Đường Thái Tông | Trường An Thành | Đối thoại chuyển tiếp nhận Chiếu Thư Tây Hành | 1500 EXP, 150 Vàng |
| **8** | Cứu Nguy Cao Gia | 12 | Trưởng Thôn Cao | Cao Lão Trang | Tiêu diệt 10 Yêu Heo Con phá hoại ruộng lúa | 2000 EXP, 200 Vàng, 1 Giáp Vải |
| **9** | Thu Phục Bát Giới | 14 | Trưởng Thôn Cao | Cao Lão Trang | Đánh bại Yêu Quái Mập (Trư Bát Giới) ở động đá | 3000 EXP, 300 Vàng, Thú cưỡi Bạch Hổ |
| **10**| Hoa Quả Sơn Viễn Chinh| 16 | Tề Thiên Đại Thánh| Hoa Quả Sơn | Đánh bại 15 Yêu Hầu Gậy biến dị | 4500 EXP, 400 Vàng |
| **11**| Khai Thác Tinh Kho | 20 | Thợ Rèn | Sau Hoa Quả Sơn| Thu thập 10 Tinh Quặng sắt cổ để đúc vũ khí | 6000 EXP, 500 Vàng, 1 Vũ Khí Cấp 20 |
| **12**| Long Cung Rẽ Nước | 26 | Bồ Tát Quan Âm | Đông Hải Long Cung| Tiêu diệt 20 Tuần Biển Dạ Soa lấy Tích Thủy Châu | 10000 EXP, 1000 Vàng |

---

## 5. Hệ Thống Phó Bản / Dungeon Chi Tiết (Dungeon System)

### A. Vườn Đào Tây Vương Mẫu (Hái Trộm Đào Tiên)
*   **Yêu cầu tham gia:** Cấp độ tối thiểu 15. Mỗi ngày tối đa 1 lượt.
*   **Cơ chế hoạt động:** 
    *   Bản đồ giới hạn thời gian (5 phút). Người chơi được đưa vào Đào Viên ngập tràn mây sương.
    *   Cứ mỗi 1 phút, một đợt *Vệ Binh Vườn Đào* sẽ xuất hiện tấn công người chơi. Sau khi tiêu diệt chúng, các quả đào tiên vàng óng sẽ rơi ngẫu nhiên trên mặt đất.
    *   Người chơi phải di chuyển (Tap-to-move) đè lên các quả đào để nhặt. Mỗi quả đào nhặt được tăng điểm tích lũy.
*   **Bảng rơi đồ (Loot Table):**
    *   Đào Tiên Lớn (tăng 2000 EXP trực tiếp).
    *   Hộp Vàng Tây Vương Mẫu (Chứa từ 100 đến 500 Vàng).

### B. Đại Náo Thiên Cung (Vượt Tháp)
*   **Yêu cầu tham gia:** Cấp độ tối thiểu 20. Không giới hạn lượt tham gia dọn tháp, reset tiến trình về Tầng 1 vào lúc 00:00 hàng ngày.
*   **Cơ chế hoạt động:**
    *   Tháp gồm 50 tầng thử thách. Mỗi tầng là 1 phòng đấu ải nhỏ.
    *   Người chơi cần tiêu diệt toàn bộ thiên binh canh giữ và 1 Thần Tướng canh ải (Boss) của tầng để mở cổng thông lên tầng kế tiếp.
    *   Độ khó (HP, Sát thương của quái) tăng thêm 5% sau mỗi tầng. Có điểm mốc lưu trữ (checkpoint) tại các tầng chia hết cho 5 (khi thất bại sẽ hồi sinh quay lại tầng mốc gần nhất).
*   **Bảng rơi đồ (Loot Table):**
    *   Đá Cường Hóa sơ/trung (Dùng để nâng cấp vũ khí tại Thợ Rèn).
    *   Trứng Tiên Sủng ngẫu nhiên (Mở ra nhận Pet Tán Tiên hoặc mảnh linh hồn).
    *   Sách kỹ năng thú cưng sơ cấp.

### C. Đại Náo Thủy Cung (Boss Thế Giới)
*   **Yêu cầu tham gia:** Cấp độ tối thiểu 30. Mở vào khung giờ cố định: 20:00 - 20:30 hàng ngày.
*   **Cơ chế hoạt động:**
    *   Map phụ bản đa người chơi (multiplayer). Tất cả người chơi trong server cùng tham chiến.
    *   Mục tiêu: Tấn công *Cự Long Thủy Quái* (Boss có HP vô hạn trong thời gian diễn ra phụ bản).
    *   Nếu người chơi tử trận, sẽ tự động hồi sinh tại Trường An sau 30 giây và có thể đi qua truyền tống cổng quay lại đánh tiếp để cộng dồn sát thương.
*   **Bảng rơi đồ (Loot Table):**
    *   *Top 1 - 3 Sát thương:* Nhận Rương Báu Long Cung chứa Ngọc Thuộc Tính bậc cao và mảnh ghép tọa kỵ hiếm (Phi Kiếm).
    *   *Top 4 - 10 Sát thương:* Nhận Rương Vàng Long Cung chứa Đá cường hóa cấp cao và vàng lớn.
    *   *Phần thưởng tham gia (toàn bộ người chơi gây >0 sát thương):* 5000 EXP và 100 Vàng.
