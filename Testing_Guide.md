# Hướng Dẫn Cấu Hình Unity & Quy Trình Chạy Thử Nghiệm Kết Nối (Integration Testing)

Tài liệu này hướng dẫn chi tiết cách cấu hình Game Object trong Unity, cách khởi chạy local Server bằng Go và quy trình thực hiện kiểm thử kết nối trao đổi dữ liệu.

---

## 1. Các bước cần xử lý thêm trên Unity Client (Unity Setup)
Trước khi nhấn nút **Play** trên Unity Editor để thử nghiệm, bạn cần gắn các file C# Script đã viết vào các Game Object tương ứng để kích hoạt vòng đời (Lifecycle) của Unity:

### Bước A: Tạo Object Quản lý mạng & Dữ liệu
1.  Trong cửa sổ **Hierarchy** của Unity, chuột phải chọn **Create Empty**, đổi tên thành `_NetworkManager`.
2.  Gắn (Drag & Drop) các Script sau vào `_NetworkManager`:
    *   `NetworkClient.cs` (Cấu hình IP: `127.0.0.1`, Port: `8080`).
    *   `ChatManager.cs`
    *   `QuestManager.cs`
    *   `MountAndPetManager.cs`
    *   `CombatManager.cs`
    *   `ConfigManager.cs`

### Bước B: Tạo Object Nhân vật (Player)
1.  Tạo một Object 2D (ví dụ: Sprite hoặc Square làm nhân vật tạm thời), đổi tên thành `Player`.
2.  Gắn Script `PlayerController.cs` vào `Player`.
3.  Cấu hình tốc độ (`speed` khoảng `5`) và kích thước ô lưới (`gridSize` là `1.0`).

### Bước C: Tạo giao diện Canvas (UI Setup)
1.  Tạo một **Canvas** trong Scene.
2.  Gắn Script `UIManager.cs` vào Canvas.
3.  Tạo các thành phần UI:
    *   Thanh máu (HP Slider) và thanh nội lực (MP Slider), kéo thả chúng vào các ô tương ứng trên Inspector của `UIManager`.
    *   Khung nhập chat (InputField) và Khung lịch sử chat (Text), kéo thả vào ô `chatInputField` và `chatHistoryText`.
    *   Tạo các nút bấm dưới màn hình (Bảng, Nhanh, Khu Phái, Shop, Sys) và trỏ sự kiện **On Click()** đến các hàm tương ứng trong `UIManager` (ví dụ: `OnClickBangButton()`, `OnClickShopButton()`).

---

## 2. Quy trình Khởi chạy Thử nghiệm cục bộ (Local Server & Redis Run)

Để hệ thống trao đổi dữ liệu hoạt động, bạn cần chạy máy chủ đệm Redis và Golang Server song song:

### Bước 1: Chạy máy chủ đệm Redis (Cache)
*   *Yêu cầu:* Game sử dụng Redis để lưu tọa độ nhân vật thời gian thực.
*   *Cách chạy:*
    *   Nếu máy bạn đã cài Redis qua Docker: Chạy lệnh `docker run -d --name redis-local -p 6379:6379 redis`
    *   Nếu chạy Redis dạng file cài đặt trên Windows: Mở thư mục chứa Redis (hoặc tải bản nén trong thư mục Downloads cũ) và chạy tệp `redis-server.exe`.

### Bước 2: Tải thư viện và Chạy Golang Server
1.  Mở terminal tại thư mục: [tayduky-server](file:///d:/AgentAI/TayDuKy2D/tayduky-server)
2.  Tải các thư viện Go cần thiết bằng các lệnh:
    ```bash
    go get github.com/go-redis/redis/v8
    go get github.com/lib/pq
    ```
3.  Khởi chạy máy chủ bằng lệnh:
    ```bash
    go run cmd/server/main.go
    ```
    *Màn hình terminal của Go sẽ báo dòng chữ:* `TCP Server listening on port 8080` và `WebSocket Server listening on port 8081`.

---

## 3. Quy trình thực nghiệm kiểm tra kết nối (How to Test)

Sau khi Server và Redis đã chạy, bạn tiến hành thực nghiệm theo các kịch bản sau:

### Kịch bản 1: Kiểm tra kết nối mạng (Socket Connect)
*   **Thao tác:** Bấm nút **Play** trên Unity Editor.
*   **Kết quả mong đợi:**
    *   Trên Console của Unity xuất hiện dòng chữ: `Connected to server at 127.0.0.1:8080`.
    *   Trên Terminal của Go Server xuất hiện dòng chữ: `New TCP Client connected: 127.0.0.1:[Port]`.

### Kịch bản 2: Kiểm tra di chuyển đồng bộ (Move Sync)
*   **Thao tác:** Di chuyển nhân vật bằng cách click chuột lên màn hình 2D hoặc nhấn các phím mũi tên. Nhân vật `Player` trên Unity sẽ trượt từng ô một.
*   **Kết quả mong đợi:**
    *   Console Unity hiển thị: `Client sent move packet: X=[Tọa độ], Y=[Tọa độ]`.
    *   Go Server nhận được gói tin JSON, tiến hành kiểm tra tốc độ, cập nhật tọa độ vào Redis và in ra log thành công.

### Kịch bản 3: Kiểm tra hệ thống Chat (Chat Sync)
*   **Thao tác:** Nhập một dòng chữ bất kỳ vào khung chat trên Unity và nhấn nút gửi hoặc phím Enter.
*   **Kết quả mong đợi:**
    *   Tin nhắn được gửi lên Server, Server nhận được và lưu lịch sử vào Redis.
    *   Khung lịch sử chat hiển thị tin nhắn có dạng: `[Thế Giới] shinichi: [Nội dung chat]`.
