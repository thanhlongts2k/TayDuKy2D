# Tài liệu Cơ chế Game & Hệ thống Thú cưỡi Tây Du Ký Mobile (J2ME)

Tài liệu này chi tiết hóa các cơ chế điều khiển, hệ thống thú cưỡi (tọa kỵ) đặc trưng và các phím chức năng của game phục dựng.

---

## 1. Cơ chế Điều khiển và Giao diện Màn hình dọc
Để giữ nguyên tính hoài cổ nhưng vẫn phù hợp với các thiết bị di động hiện đại (iOS, Android) và chơi trên Web, cơ chế điều khiển sẽ được tối ưu hóa:
*   **Chạm màn hình di chuyển (Tap-to-Move):** Người chơi chạm vào bất kỳ điểm nào trên bản đồ 2D dạng lưới (Tilemap), nhân vật sẽ tự động tìm đường đi tới đó.
*   **D-pad ảo (Virtual D-pad):** Cung cấp tùy chọn hiển thị một nút điều hướng tròn (hoặc 4 phím mũi tên) mờ ở góc dưới màn hình, mô phỏng lại bàn phím số/Joystick của điện thoại Java cổ.
*   **Tương tác thông minh (Smart Interact):** Khi đến gần NPC hoặc vật phẩm nhiệm vụ, một nút hành động dạng nổi (hoặc phím tắt nhanh) sẽ xuất hiện để người chơi bấm nói chuyện/thu thập.

---

## 2. Hệ thống Môn phái và Cảnh giới (Factions & Realms)
Game chia làm **3 thế lực chính (Giáo phái/Môn phái)** mà người chơi có thể gia nhập quanh cấp độ 10:
1.  **Thần Tộc (Gods):** Đại diện cho Thiên giới, sở hữu phép thuật thanh cao, sát thương phép diện rộng và khả năng phòng thủ phép mạnh.
2.  **Ma Tộc (Demons):** Đại diện cho thế lực U Minh/Địa ngục, thiên về sát thương chí mạng vật lý và bạo kích tầm gần.
3.  **Yêu Tộc (Monsters):** Đại diện cho yêu ma nhân gian, sở hữu sinh lực (HP) cao, khả năng sinh tồn phòng thủ vật lý cực lớn và phản sát thương.

*Hệ thống Cảnh giới:* Nhân vật tu luyện thăng tiến qua các cảnh giới (Huyền Tiên, Chân Tiên, Kim Tiên, Thiên Tiên...) để đột phá thuộc tính và nhận kỹ năng nâng cấp.

---

## 3. Hệ thống Thú cưỡi (Tọa kỵ) đặc trưng
Hệ thống thú cưỡi trong Tây Du Ký Mobile không chỉ tăng tốc độ di chuyển trên bản đồ mà còn gia tăng trực tiếp thuộc tính sức mạnh cho nhân vật (HP, MP, Công, Thủ, Tốc độ né tránh).

Dựa trên các tư liệu hình ảnh, danh sách thú cưỡi được thiết kế như sau:

| Tên Thú Cưỡi | Hình ảnh mô tả | Cấp độ yêu cầu | Thuộc tính gia tăng chính | Loại di chuyển |
| :--- | :--- | :--- | :--- | :--- |
| **U Minh Bạch Hổ** | Hổ trắng sọc đen, tư thế uy dũng, giáp chân vàng. | Cấp 30 | Tăng Tốc độ chạy (+30%), Sát thương Vật lý. | Đi bộ / Mặt đất |
| **Lục Nha Bạch Tượng** | Voi trắng đeo kiệu thảm thêu đỏ thắt đai vàng quý phái. | Cấp 30 | Tăng HP tối đa (+15%), Phòng thủ Vật lý (+10%). | Đi bộ / Mặt đất |
| **Thanh Sư Thần Thú** | Sư tử bờm vàng, mang giáp đen thép, đuôi bò cạp lửa. | Cấp 30 | Tăng HP tối đa (+10%), Sát thương phép thuật. | Đi bộ / Mặt đất |
| **Hỏa Kỳ Lân** | Kỳ lân thân xanh đỏ, đầu có sừng phát sáng, quanh chân có lửa. | Cấp 35 | Tăng Tốc độ chạy (+40%), Tỷ lệ Chí mạng (+5%). | Chạy nhanh / Lướt đất |
| **Bạch Hạc Tiên Nhân** | Chim hạc trắng cao ráo, sải cánh rộng, mỏ đỏ. | Cấp 35 | Tăng Né tránh (+8%), Kháng phép (+5%). | Bay thấp / Vượt vực |
| **Hỏa Phượng Hoàng** | Phượng hoàng lửa rực rỡ, đuôi dài phát sáng tia lửa đỏ cam. | Cấp 40 | Tăng Tốc độ chạy (+50%), Sát thương Lửa/Chí mạng. | Bay cao / Vượt địa hình |
| **Lục Bảo Phi Kiếm** | Thanh kiếm tiên phát sáng xanh lục lơ lửng, người chơi đứng trên đó. | Cấp 30 (Sự kiện) | Tăng Tốc độ chạy (+35%), Tốc độ xuất chiêu (+5%). | Bay thấp / Vượt địa hình |
| **Cân Đẩu Vân** | Đám mây vàng bồng bềnh tỏa hào quang kim sắc của Tôn Ngộ Không. | Cấp 50 (Thượng đẳng) | Tăng Tốc độ chạy (+60%), Né tránh (+15%). | Bay tự do / Mọi địa hình |
| **Kim Mao Hống** | Thần thú giống chó bờm vàng rực lửa, tọa kỵ của Quan Âm Bồ Tát.| Cấp 45 | Tăng Sát thương Phép (+12%), Sát thương Lửa (+10%). | Đi bộ / Mặt đất |
| **Ngũ Sắc Thần Lộc** | Chú hươu thần có sừng 5 màu tỏa ánh sáng dịu nhẹ. | Cấp 40 | Tăng HP tối đa (+20%), Kháng tất cả thuộc tính (+8%). | Chạy nhanh / Lướt đất |
| **Cửu Linh Nguyên Thánh** | Sư tử 9 đầu oai phong lẫm liệt, tọa kỵ của Thái Ất Thiên Tôn. | Cấp 55 (Huyền thoại) | Tăng Sức mạnh (+15%), Sát thương Vật lý (+15%). | Đi bộ / Mặt đất |


---

## 4. Cơ chế Chiến đấu (Combat System) & Phụ bản
Đề xuất sử dụng lối chơi **Đánh theo lượt (Turn-based)** nguyên bản:
*   **Đội hình chiến đấu:** Gồm 1 Chủ tướng (người chơi) đứng ở giữa và tối đa các Tướng hỗ trợ/Tiên sủng (Pets) được bố trí xung quanh trợ chiến.
*   **Hệ thống Tiên Sủng:** Hơn 700 loại tiên sủng có thể bắt, nuôi dưỡng, học kỹ năng riêng biệt để cùng ra trận.
*   **Các phụ bản đặc trưng:**
    *   *Hái Trộm Đào Tiên (Đông Hải/Thiên Đình):* Phụ bản kiếm EXP và Vàng hàng ngày.
    *   *Đại Náo Thiên Cung:* Phụ bản vượt ải leo tháp thử thách sức mạnh đội hình.
    *   *Đại Náo Thủy Cung:* Săn Boss thế giới nhận trang bị hiếm.


---

## 4. Chi tiết các Phím chức năng chính dưới màn hình
1.  **Bảng:**
    *   *Hành trang (Inventory):* Xem và trang bị Vũ khí, Giáp, Pháp bảo, Đạo cụ hồi phục.
    *   *Thông tin nhân vật:* Xem Cảnh giới tu tiên (như Huyền Tiên, Kim Tiên...), điểm tiềm năng (Sức mạnh, Trí lực, Thể lực, Thân pháp) và đổi danh hiệu.
    *   *Bang hội:* Quản lý thành viên bang, cống hiến, nâng cấp kỹ năng bang hội.
2.  **Nhanh:**
    *   *Bảng Phím tắt:* Cài đặt nhanh các kỹ năng hoặc bình dược phẩm vào các phím số/phím ảo để sử dụng ngay trong trận chiến chỉ với 1 chạm.
    *   *Thiết lập Auto:* Cấu hình tự động bơm HP/MP khi xuống dưới x%, tự động nhặt đồ phẩm chất nào.
3.  **Khu Phái:**
    *   *Chuyển Kênh:* Cho phép người chơi di chuyển giữa các Sub-server/Kênh (Channel 1, 2, 3...) tại cùng một bản đồ để tránh quá tải hoặc săn boss.
    *   *Truyền tống Môn phái:* Dịch chuyển nhanh về tổng đà của Môn phái/Giáo phái đã gia nhập để nhận nhiệm vụ môn phái hàng ngày.
4.  **Shop:**
    *   *Kỳ Trân Các:* Cửa hàng vật phẩm đặc biệt (thú cưỡi cao cấp, thời trang, thẻ nhân đôi EXP).
    *   *Tiệm Tiên Sủng:* Mua trứng tiên sủng, thức ăn cho thú nuôi và đan dược tiến hóa.
5.  **Sys (System):**
    *   Thiết lập bật/tắt nhạc nền, âm thanh hiệu ứng kỹ năng.
    *   Tùy chọn chất lượng đồ họa (Pixel sắc nét hoặc làm mịn khử răng cưa).
    *   Truy cập trung tâm trợ giúp và nạp thẻ.
