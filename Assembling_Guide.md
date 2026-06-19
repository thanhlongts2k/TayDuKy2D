# HƯỚNG DẪN CHI TIẾT TỪNG BƯỚC: LẮP RÁP CÁC SCRIPT C# VÀO GAME OBJECTS (UNITY)

Tài liệu này cung cấp hướng dẫn chi tiết để bạn thiết lập và liên kết các file Script C# vào các Game Object trong Unity Editor cho dự án **Tây Du Ký 2D**.

Có **2 phương pháp** để thực hiện:
*   **Cách 1 (Khuyên dùng):** Sử dụng công cụ tự động lắp ráp (chỉ mất 1-click).
*   **Cách 2 (Thủ công):** Tự tay tạo các Game Object và kéo thả các thành phần trong Inspector để hiểu sâu hơn về cấu trúc.

---

## CÁCH 1: TỰ ĐỘNG LẮP RÁP BẰNG MENU EDITOR (1-CLICK)

Chúng tôi đã lập trình sẵn một kịch bản Editor Script (`SetupTestScene.cs`) để tự động hóa toàn bộ việc tạo Game Object, gắn script và cấu hình liên kết.

### Các bước thực hiện:
1. Mở dự án **tayduky-client** bằng **Unity Editor** (Khuyên dùng phiên bản Unity 2021.3 LTS trở lên).
2. Chờ Unity load xong Assets.
3. Trên thanh công cụ trên cùng của Unity Editor, bạn sẽ thấy menu **Tools**.
4. Click vào **Tools** -> Chọn **Tây Du Ký** -> Chọn **Tự Động Lắp Ráp Scene Thử Nghiệm**.
5. Hệ thống sẽ hiển thị một hộp thoại thông báo lắp ráp thành công.
6. Vào thư mục `Assets/Scenes/` trong cửa sổ *Project*, bạn sẽ thấy scene mới tên là **WorldScene.unity**.
7. Nhấp đúp vào **WorldScene** để mở scene này lên. Bạn đã có sẵn một scene hoàn chỉnh để chạy thử nghiệm!

---

## CÁCH 2: LẮP RÁP THỦ CÔNG TỪNG BƯỚC (inspector drag & drop)

Nếu bạn muốn tự tay thiết lập scene từ đầu hoặc tích hợp vào scene hiện có, hãy làm theo quy trình chi tiết dưới đây:

### BƯỚC 1: Tạo Object Quản lý mạng & Dữ liệu (`_NetworkManager`)
Đối tượng này đóng vai trò "trung não", quản lý kết nối TCP Socket và đồng bộ các hệ thống (Chat, Combat, Nhiệm vụ, Thú cưỡi, Config).

1. Trong cửa sổ **Hierarchy** (bên trái màn hình), nhấp chuột phải vào vùng trống -> Chọn **Create Empty**.
2. Đổi tên Game Object vừa tạo thành: `_NetworkManager`.
3. Nhấp chọn `_NetworkManager`. Tại cửa sổ **Inspector** (bên phải màn hình), nhấn **Add Component**.
4. Tìm kiếm và thêm lần lượt các Script sau:
   *   `NetworkClient` (Nhập IP: `127.0.0.1`, Port: `8080`).
   *   `ConfigManager`
   *   `ChatManager`
   *   `QuestManager`
   *   `MountAndPetManager`
   *   `CombatManager`

> [!NOTE]
> Bạn cũng có thể kéo trực tiếp các file script này từ thư mục `Assets/Scripts/Network/` và `Assets/Scripts/Managers/` trong cửa sổ *Project* thả vào cửa sổ *Inspector* của `_NetworkManager`.

---

### BƯỚC 2: Tạo Object Nhân vật chính (`Player`)
Đối tượng này đại diện cho nhân vật người chơi trên bản đồ 2D.

1. Trong cửa sổ **Hierarchy**, nhấp chuột phải -> Chọn **2D Object** -> Chọn **Sprites** -> Chọn **Square** (hoặc Capsule/Circle tùy ý làm hình đại diện tạm thời).
2. Đổi tên Game Object vừa tạo thành: `Player`.
3. Nhấp chọn `Player`, tại cửa sổ **Inspector**, nhấn **Add Component** và thêm script:
   *   `PlayerController`
4. Cấu hình các thông số trong `PlayerController` ở Inspector:
   *   `Speed`: Nhập `5` (tốc độ di chuyển mịn).
   *   `Grid Size`: Nhập `1` (kích thước mỗi ô lưới 2D là 1x1 đơn vị).

---

### BƯỚC 3: Tạo Canvas và Thiết lập giao diện UI (`Canvas`)
Canvas chứa toàn bộ giao diện người dùng như thanh HP/MP, khung chat và các phím chức năng Nokia cổ điển.

1. Trong cửa sổ **Hierarchy**, nhấp chuột phải -> Chọn **UI** -> Chọn **Canvas**.
2. Nhấp chọn đối tượng **Canvas** vừa tạo. Tại cửa sổ **Inspector**:
   *   Nhấn **Add Component**, chọn script `UIManager`.
3. Tạo hệ thống thanh máu & nội lực (Top-Left):
   *   Chuột phải vào **Canvas** -> Chọn **UI** -> Chọn **Panel**, đặt tên là `StatsPanel`. Căn góc neo (Anchor) của Panel này ở góc trên bên trái (Top-Left).
   *   Chuột phải vào `StatsPanel` -> Tạo **UI** -> **Slider** (Đặt tên là `HPSlider`). Đổi màu của phần *Fill* thành màu đỏ.
   *   Chuột phải vào `StatsPanel` -> Tạo **UI** -> **Slider** (Đặt tên là `MPSlider`). Đổi màu của phần *Fill* thành màu xanh dương.
   *   Tạo 2 đối tượng **UI** -> **Text** đặt tên là `NameText` (hiển thị tên) và `LevelText` (hiển thị cấp độ).
4. Tạo khung chat (Bottom):
   *   Chuột phải vào **Canvas** -> Tạo **UI** -> **Panel**, đặt tên là `ChatPanel`. Căn góc neo ở cạnh dưới (Bottom-Stretch).
   *   Chuột phải vào `ChatPanel` -> Tạo **UI** -> **Text**, đặt tên là `ChatHistoryText` (hiển thị lịch sử chat).
   *   Chuột phải vào `ChatPanel` -> Tạo **UI** -> **Input Field**, đặt tên là `ChatInputField` (khung nhập chat).

---

### BƯỚC 4: Kết nối các tham chiếu trong `UIManager` (Rất quan trọng!)
Để Script điều khiển được UI, bạn cần chỉ định rõ các đối tượng UI vừa tạo vào Script.

1. Nhấp chọn Game Object **Canvas** (nơi chứa component `UIManager`).
2. Nhìn vào phần Script `UIManager` trên Inspector, bạn sẽ thấy các ô trống (None) chờ kéo thả.
3. Kéo các đối tượng ở Hierarchy thả vào ô tương ứng trong Inspector của `UIManager`:
   *   Kéo `HPSlider` thả vào ô **Hp Slider**
   *   Kéo `MPSlider` thả vào ô **Mp Slider**
   *   Kéo `LevelText` thả vào ô **Level Text**
   *   Kéo `NameText` thả vào ô **Name Text**
   *   Kéo `ChatHistoryText` thả vào ô **Chat History Text**
   *   Kéo `ChatInputField` thả vào ô **Chat Input Field**
   *   Kéo `ChatPanel` thả vào ô **Chat Panel**

---

### BƯỚC 5: Thiết lập Event gửi tin nhắn chat khi nhấn Submit
Khi gõ xong tin nhắn và bấm Enter hoặc nút gửi, UI cần gọi hàm để truyền dữ liệu lên mạng.

1. Nhấp chọn Game Object `ChatInputField` dưới `ChatPanel`.
2. Trong Inspector của `ChatInputField`, cuộn xuống dưới cùng tìm mục **On End Edit (String)** hoặc **On Submit**.
3. Nhấn vào dấu **+** để thêm sự kiện mới.
4. Kéo thả Game Object **Canvas** (chứa `UIManager`) vào ô chọn đối tượng (ô nằm dưới dòng *Runtime Only*).
5. Nhấp vào menu đổ xuống bên cạnh (chữ *No Function*) -> Chọn **UIManager** -> Chọn **OnSendChatSubmit ()**.

---

### BƯỚC 6: Thiết lập các nút bấm Nokia Retro (Bảng, Nhanh, Khu Phái, Shop, Sys)
Cấu hình các phím điều hướng nhanh mô phỏng phím bấm J2ME.

1. Dưới **Canvas**, tạo 5 nút bấm **UI** -> **Button**, đặt tên tương ứng: `BtnBang`, `BtnNhanh`, `BtnKhuPhai`, `BtnShop`, `BtnSys`.
2. Với mỗi nút bấm, chọn nút đó, cuộn xuống mục **On Click ()** ở Inspector:
   *   Nhấn dấu **+** để thêm sự kiện.
   *   Kéo Game Object **Canvas** thả vào ô đối tượng.
   *   Tại menu chọn Function, chọn **UIManager** và trỏ đến hàm tương ứng:
       *   `BtnBang` -> Trỏ đến `OnClickBangButton()`
       *   `BtnNhanh` -> Trỏ đến `OnClickNhanhButton()`
       *   `BtnKhuPhai` -> Trỏ đến `OnClickKhuPhaiButton()`
       *   `BtnShop` -> Trỏ đến `OnClickShopButton()`
       *   `BtnSys` -> Trỏ đến `OnClickSysButton()`

---

## KIỂM TRA & VẬN HÀNH

Sau khi đã hoàn thành lắp ráp (bằng cách 1 hoặc cách 2), bạn thực hiện chạy thử nghiệm:

1.  **Chạy server giả lập (Python Mock Server):**
    *   Mở terminal tại máy của bạn.
    *   Chạy lệnh: `python D:\AgentAI\TayDuKy2D\server.py`
    *   Server sẽ lắng nghe tại cổng `8080`.
2.  **Chạy Unity Client:**
    *   Trong Unity Editor, nhấn nút **Play** (hình tam giác màu đen ở trên cùng).
    *   Nhấp chuột lên màn hình để nhân vật di chuyển theo ô lưới (Grid Move). Xem log di chuyển trên Console Unity và màn hình Server.
    *   Nhấp vào khung chat, gõ tin nhắn và nhấn gửi để xem tin nhắn đồng bộ lên màn hình.
