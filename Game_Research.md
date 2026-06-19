# Tài liệu Nghiên cứu & Phân tích Lịch sử Game Tây Du Ký Mobile (J2ME)

Tài liệu này tổng hợp lịch sử hoạt động, đặc trưng giao diện và trải nghiệm người dùng của tựa game **Tây Du Ký Mobile** từng do ME Corp phát hành năm 2013, làm cơ sở cho kế hoạch phục dựng dự án.

---

## 1. Lịch sử và Bối cảnh Phát hành
*   **Tên game:** Tây Du Ký Mobile (thường gọi tắt là Tây Du Mobile).
*   **Nhà phát hành tại Việt Nam:** ME Corp (Mobile Entertainment Corporation).
*   **Năm ra mắt:** Khoảng tháng 10 năm 2013.
*   **Nền tảng hỗ trợ gốc:** Java (J2ME) - định dạng file `.jar` / `.jad` và hệ điều hành Android thời kỳ đầu.
*   **Tình trạng hiện tại:** Đã đóng cửa máy chủ chính thức từ nhiều năm trước. Không còn phiên bản online chính thức hoạt động, chỉ còn các ảnh chụp màn hình kỷ niệm và tệp lưu trữ.
*   **Tầm ảnh hưởng:** Là một trong những game MMORPG 2D màn hình dọc có số lượng người chơi cực kỳ đông đảo trên các dòng máy Nokia Symbian (S40, S60), nhờ cấu hình siêu nhẹ, lối chơi lôi cuốn và khả năng tương thích cao với mạng 2G/3G thời kỳ đó.

---

## 2. Đặc trưng Giao diện Màn hình dọc (Portrait UI)
Khác với các game PC hoặc game mobile hiện đại chạy màn hình ngang (Landscape), Tây Du Ký Mobile sử dụng giao diện dọc (Portrait) tối ưu cho việc điều khiển bằng một tay hoặc bàn phím số (T9) trên điện thoại Nokia cổ điển.

### Các thành phần chính trên giao diện:
1.  **Góc trên bên trái (Avatar & Status):**
    *   Ảnh chân dung đại diện của người chơi.
    *   Cấp độ nhân vật (hiển thị bằng số màu vàng, ví dụ: `32`).
    *   Thanh máu (HP - Màu đỏ) và thanh năng lượng (MP - Màu xanh dương) xếp song song nằm ngang.
    *   Nút **"Mở VIP"** hoặc **"VIP [Cấp độ]"** nằm ngay cạnh thanh máu để truy cập nhanh tính năng nạp thẻ/đặc quyền.
2.  **Góc trên bên phải (Mini-map):**
    *   Một bản đồ tròn hiển thị sơ đồ khu vực hiện tại.
    *   Các dấu chấm màu sắc: Dấu chấm xanh dương biểu thị người chơi khác/NPC, dấu chấm đỏ biểu thị quái vật/mục tiêu nhiệm vụ.
3.  **Khu vực trung tâm (Viewport):**
    *   Hiển thị thế giới game dưới dạng lưới gạch (Tilemap) 2D trực quan.
    *   Nhân vật có thể đứng yên hoặc di chuyển cưỡi tọa kỵ (như Bạch Hổ, Hỏa Kỳ Lân, hoặc đứng trên Phi Kiếm).
    *   Tên nhân vật người chơi được viết bằng màu đỏ với định dạng đặc biệt (ví dụ: `<<shinichi>>`).
    *   NPC và quái vật có tên màu xanh lá cây hoặc màu xanh dương nổi bật (ví dụ: `Đồng Tử`, `Không Tiên`, `Tiểu Toàn Phong`).
    *   Nhiệm vụ khả dụng sẽ có dấu chấm than màu vàng quay tròn ngay trên đầu NPC.
4.  **Góc dưới bên phải (Phím Zoom/Kính lúp):**
    *   Nút hình chiếc kính lúp dấu cộng/trừ cho phép phóng to/thu nhỏ khung hình để phù hợp với các độ phân giải màn hình khác nhau (như 240x320, 320x240).
5.  **Thanh chức năng chính (Bottom Menu):**
    *   Nằm ngay phía trên khung chat, gồm các phím tắt truy cập nhanh:
        *   **Bảng (Bang hội / Bảng xếp hạng / Hành trang):** Quản lý bang hội, vật phẩm và thông tin cá nhân.
        *   **Nhanh (Phím tắt / Auto chiến đấu):** Bật/tắt tự động đánh quái hoặc gán phím tắt nhanh cho kỹ năng/bình máu.
        *   **Khu Phái (Chuyển kênh / Môn phái):** Chuyển đổi giữa các kênh (Khu vực) tránh đông đúc hoặc quay về bang hội/môn phái.
        *   **Shop (Cửa hàng):** Mua sắm trang bị, tiên sủng, dược phẩm bằng tiền trong game hoặc KNB.
        *   **Sys (Hệ thống / Cài đặt):** Cấu hình âm thanh, đồ họa, đăng xuất hoặc đổi nhân vật.
6.  **Khung Chat dưới cùng:**
    *   Hiển thị dòng hội thoại của người chơi khác hoặc thông báo hệ thống màu vàng/trắng.
    *   Nút **"Nói"** ở góc phải để mở bàn phím nhập nội dung chat.

---

## 3. Trải nghiệm Người dùng (UX) đặc trưng
*   **Điều khiển dễ dàng:** Người chơi di chuyển bằng cách nhấn các phím hướng `2, 4, 6, 8` hoặc phím điều hướng Joystick trên máy Java. Đối với phiên bản phục dựng, cơ chế này sẽ được thay thế bằng D-pad ảo hoặc chạm màn hình (Tap-to-move).
*   **Tiết kiệm băng thông tối đa:** Nhờ tối ưu hóa đồ họa 2D Pixel dạng lưới, dung lượng tải và lượng dữ liệu truyền nhận qua mạng cực kỳ thấp, giúp game chạy mượt mà ngay cả khi kết nối mạng yếu.
*   **Tính cộng đồng cao:** Cơ chế chat hệ thống và thế giới được cập nhật liên tục ở nửa dưới màn hình giúp người chơi luôn giữ kết nối, dễ dàng lập tổ đội đi phụ bản hoặc trao đổi mua bán.
