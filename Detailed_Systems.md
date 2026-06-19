# Tài liệu Nghiên cứu Chuyên sâu Hệ thống Bản đồ, NPC, Tiên Sủng & Nhiệm vụ

Tài liệu này cung cấp thiết kế chi tiết về các hệ thống bản đồ (map) theo tuyến cấp độ, danh sách NPC chức năng, hệ thống phân cấp Tiên sủng (Pet) và cấu trúc nhiệm vụ phụ bản kinh điển của tựa game **Tây Du Ký Mobile (J2ME)**.

---

## 1. Hệ thống Bản đồ & Tuyến Cấp độ (Map & Progression)

Hành trình của người chơi được thiết kế mạch lạc, đi từ cõi Tiên giới (bắt đầu biến cố) xuống đến Nhân giới (Đường đi thỉnh kinh) và các vùng đất Yêu ma hiểm trở:

### A. Tuyến Bản đồ Tiên Giới (Level 1 - 9)
*   **Hội Bàn Đào:** Bản đồ hướng dẫn tân thủ (Tutorial Map). Nơi diễn ra biến cố Tề Thiên Đại Thánh náo loạn hội đào tiên, người chơi thực hiện các nhiệm vụ sơ cấp để hiểu cốt truyện.
*   **Dao Trì:** Nơi chịu tội phạt của ba vị thần tướng (Ngộ Không, Bát Giới, Sa Tăng). Người chơi giải cứu/giúp đỡ họ để mở ra thiên mệnh.
*   **Nam Thiên Môn & Thiên Cung:** Bản đồ luyện cấp đầu tiên (Level 5 - 9). Gồm các hòn đảo lửng lơ trên tầng mây, là nơi bái kiến tiên nhân để học võ công cơ bản và dịch chuyển xuống trần gian.

### B. Tuyến Bản đồ Nhân Giới & Thỉnh Kinh (Level 10+)
*   **Trường An Thành (Level 10 - 15):** Kinh đô nhà Đường sầm uất. Nơi người chơi hạ phàm đầu tiên, gặp gỡ Đường Tăng bắt đầu hành trình Tây Du và gia nhập một trong 3 tộc: **Thần, Ma, Yêu**.
*   **Cao Lão Trang (Level 15 - 20):** Map làng quê thanh bình nhưng bị quấy phá bởi yêu quái (nơi thu phục Thiên Bồng/Trư Bát Giới).
*   **Hoa Quả Sơn & Thủy Liêm Động (Level 20 - 29):** Vương quốc loài khỉ, nhiều cây cối thác nước. Phân khu *Sau Hoa Quả Sơn* là bản đồ khai thác mỏ tài nguyên (quặng, đá quý, tinh kho).
*   **Đông Hải Long Cung (Level 30 - 35):** Thế giới dưới nước (Thủy cung), có các loài tôm cá tuần tra. Người chơi xuống đây tìm kiếm binh khí và pháp bảo.
*   **Hỏa Diệm Sơn (Level 36 - 45):** Map vùng đất lửa khô cằn đầy dung nham, khí hậu nóng bức và nhiều ma quỷ lửa.
*   **Sư Đà Lĩnh (Level 46 - 55):** Map hang động và rừng rú hiểm trở, thánh địa của các đại ma đầu nguy hiểm nhất.

---

## 2. Hệ thống NPC Chức năng và Cốt truyện (NPC Directory)

Mỗi bản đồ có các NPC phục vụ cho các hoạt động khác nhau của người chơi:

| Tên NPC | Vị trí bản đồ | Chức năng chính |
| :--- | :--- | :--- |
| **Đồng Tử** | Hội Bàn Đào / Dao Trì | Hướng dẫn tân thủ, phát trang bị gỗ ban đầu. |
| **Không Tiên** | Nam Thiên Môn | Hướng dẫn Cảnh giới tu tiên, làm lễ nhập tộc (Thần - Ma - Yêu). |
| **Đường Thái Tông** | Trường An Thành | Giao nhiệm vụ chính tuyến ban chiếu thư đi Tây Thiên. |
| **Thần Y** | Trường An / Cao Lão Trang | Hồi phục HP/MP miễn phí cho người chơi dưới cấp 15, bán tiên dược. |
| **Tiệm Rèn (Thiết Tượng)** | Trường An Thành | Cường hóa vũ khí, khảm nạm ngọc thuộc tính, sửa chữa trang bị hỏng. |
| **Quản Kho** | Trường An Thành | Lưu trữ đồ đạc chung của tài khoản (Rương chứa đồ). |
| **Đường Tăng** | Trường An -> Thỉnh Kinh | NPC cốt truyện chính, di chuyển theo tiến độ nhiệm vụ của người chơi. |
| **Vương Mẫu Nương Nương**| Thiên Cung | NPC phụ trách mở phụ bản *Hái Trộm Đào Tiên*. |
| **Đông Hải Long Vương** | Đông Hải Long Cung | NPC phụ trách mở phụ bản *Đại Náo Thủy Cung* săn boss. |

---

## 3. Hệ thống Tiên Sủng (Pet System)

Tiên sủng là linh hồn chiến đấu hỗ trợ đắc lực cho Chủ tướng trong các trận đánh turn-based. Hệ thống gồm hơn 700 loài được chia làm 3 cấp bậc chính dựa trên phẩm chất và tư chất:

### A. Phân cấp Tiên Sủng
1.  **Trân Thú (Cấp Thường):** 
    *   *Mô tả:* Các loài thú hoang dã tu luyện sơ cấp như Thỏ Ngọc, Yêu Hầu, Tiểu Miêu, Hoa Yêu.
    *   *Tư chất:* Thấp, dễ bắt bằng lưới bắt thú thông thường ở các map ngoài thành. Phù hợp cho giai đoạn tân thủ (Level 1 - 15).
2.  **Tán Tiên (Cấp Trung):**
    *   *Mô tả:* Các sinh vật tiên linh có pháp lực hoặc tiểu yêu quái có tên tuổi như Tiểu Toàn Phong, Tuần Biển Dạ Soa, Thiết Phiến Yêu, Hắc Linh Miêu.
    *   *Tư chất:* Trung bình khá, có kỹ năng khống chế hoặc trị thương tốt. Thu phục thông qua đánh phụ bản hoặc ấp trứng tiên sủng hiếm.
3.  **Kim Tiên (Cấp Thần):**
    *   *Mô tả:* Thần thú thượng giới hoặc đại yêu vương có sức mạnh vô song như Kỳ Lân Con, Tiểu Phượng Hoàng, Thần Long Đại Đế, Kim Sí Điêu Con.
    *   *Tư chất:* Cực cao, sở hữu các kỹ năng tối thượng (thi triển phép diện rộng, hồi sinh chủ tướng, tăng giáp bất tử tạm thời). Nhận được từ việc ghép đá hồn tiên sủng hoặc tham gia sự kiện đặc biệt.

### B. Cơ chế Tiến hóa và Nâng cấp Pet
*   **Tăng cấp (Level Up):** Pet ra trận cùng chủ tướng sẽ nhận điểm kinh nghiệm để thăng cấp.
*   **Học kỹ năng:** Pet có các ô kỹ năng trống (tối đa 6 ô). Người chơi mua *Sách Kỹ Năng* trong Kỳ Trân Các hoặc làm nhiệm vụ để dạy cho Pet các kỹ năng chủ động (tấn công, đóng băng, hồi máu) hoặc bị động (tăng HP, tăng chí mạng cho chủ).
*   **Tẩy tủy (Reset tư chất):** Dùng *Tẩy Tủy Đan* để cài đặt lại các chỉ số tư chất (Sức mạnh, Thể lực, Trí tuệ...) của Pet về trạng thái tốt nhất (Tư chất trác việt/Thần thánh).

---

## 4. Hệ thống Nhiệm vụ & Phụ bản Kinh điển

Chuỗi hoạt động hàng ngày giúp giữ chân người chơi:

### A. Phân loại Nhiệm vụ (Quest System)
*   **Nhiệm vụ Chính tuyến (Hộ tống Đường Tăng):** Đi theo cốt truyện thỉnh kinh, giải quyết các khó khăn tại mỗi trạm dừng chân (vượt kiếp nạn).
*   **Nhiệm vụ Bang hội:** Cống hiến tài nguyên (gỗ, đá từ mỏ khai thác) để nhận điểm cống hiến đổi trang bị quý.
*   **Nhiệm vụ Tu tiên (Hàng ngày):** Nhận từ NPC Không Tiên, tiêu diệt số lượng quái vật chỉ định để tích lũy điểm ngộ đạo tăng cảnh giới Huyền Tiên.

### B. Cơ chế Phụ bản (Dungeons)
1.  **Hái Trộm Đào Tiên (Đào Viên):**
    *   *Cơ chế:* Người chơi dịch chuyển vào vườn đào tiên của Vương Mẫu. Trong thời gian giới hạn (5 phút), phải đánh bại các Vệ Binh Vườn Đào và thu hoạch quả đào rơi xuống.
    *   *Phần thưởng:* Lượng lớn EXP và Vàng.
2.  **Đại Náo Thiên Cung (Tháp Thử Thách):**
    *   *Cơ chế:* Phụ bản leo tầng (Tower Defence/Tower Rush). Mỗi tầng có các Thiên Binh Thiên Tướng canh giữ mạnh dần lên. Đội hình người chơi và Pet phải vượt qua từng tầng để nhận quà.
    *   *Phần thưởng:* Trang bị quý phẩm chất Lam/Tím, Tẩy Tủy Đan, Đá nâng cấp thú cưỡi.
3.  **Đại Náo Thủy Cung (Boss Thế Giới):**
    *   *Cơ chế:* Diễn ra vào khung giờ cố định hàng ngày tại Đông Hải Long Cung. Tất cả người chơi trên server cùng tham gia tấn công Boss Long Vương hoặc Cự Long Thủy Quái. Sát thương gây ra càng lớn, phần thưởng càng cao.
    *   *Phần thưởng:* Ngọc thuộc tính cấp cao, Đá hồn ghép Pet Kim Tiên.
