# Checklist Triển Khai Source Code Client (Unity C#)

Tài liệu này liệt kê tuần tự các cấu trúc mã nguồn Client cần lập trình bổ sung để hoàn thiện bản mẫu thử nghiệm (Prototype) game Tây Du Ký Mobile.

---

## 1. Danh Sách Các Module Cần Bổ Sung & Trạng Thái

- [x] **NetworkClient.cs** (Kết nối mạng TCP Socket đa luồng)
- [x] **PlayerController.cs** (Điều khiển di chuyển ô lưới 2D, Tap-to-move)
- [x] **ConfigManager.cs** (Nạp cấu hình JSON Items/Quests từ Resources)
- [x] **CombatManager.cs** (Quản lý trạng thái chiến đấu turn-based)
- [x] **UIManager.cs** (Điều khiển giao diện màn hình dọc, thanh HP/MP, khung chat và các bảng chức năng)
- [x] **ChatManager.cs** (Xử lý gửi/nhận tin nhắn chat Thế giới, Bang hội, mật)
- [x] **QuestManager.cs** (Quản lý tiến trình nhiệm vụ thỉnh kinh và tương tác NPC dã ngoại)
- [x] **MountAndPetManager.cs** (Quản lý triệu hồi Tiên sủng và cưỡi Tọa kỵ)

---

## 2. Kế Hoạch Thực Thi Tuần Tự (Step-by-Step Execution Plan)

### Bước 1: Lập trình [UIManager.cs](file:///d:/AgentAI/TayDuKy2D/tayduky-client/Assets/Scripts/UI/UIManager.cs)
*   **Mục tiêu:** Quản lý giao diện màn hình dọc của game.
*   **Nghiệp vụ:** 
    *   Cập nhật Sliders HP/MP và text level góc trái màn hình.
    *   Xử lý bấm phím dưới màn hình: Bảng (Hành trang/Thông tin), Nhanh (Auto), Khu Phái (Truyền tống), Shop, Sys (Hệ thống).
    *   Hiển thị hộp nhập chat và danh sách tin nhắn chat.

### Bước 2: Lập trình [ChatManager.cs](file:///d:/AgentAI/TayDuKy2D/tayduky-client/Assets/Scripts/Managers/ChatManager.cs)
*   **Mục tiêu:** Quản lý truyền thông tin chat giữa Client và Server Go.
*   **Nghiệp vụ:**
    *   Gửi tin nhắn (gói tin JSON Action ID `1005`).
    *   Tiếp nhận tin nhắn chat từ `NetworkClient` và hiển thị trực tiếp lên khung chat của `UIManager`.

### Bước 3: Lập trình [QuestManager.cs](file:///d:/AgentAI/TayDuKy2D/tayduky-client/Assets/Scripts/Managers/QuestManager.cs)
*   **Mục tiêu:** Đồng bộ tiến trình nhiệm vụ.
*   **Nghiệp vụ:**
    *   Theo dõi tiến trình giết quái/nhặt đồ của nhiệm vụ đang làm.
    *   Tương tác với NPC (như Đồng Tử, Không Tiên) để trả nhiệm vụ và nhận thưởng.

### Bước 4: Lập trình [MountAndPetManager.cs](file:///d:/AgentAI/TayDuKy2D/tayduky-client/Assets/Scripts/Managers/MountAndPetManager.cs)
*   **Mục tiêu:** Quản lý nâng cao thú cưỡi và pet.
*   **Nghiệp vụ:**
    *   Đồng bộ thuộc tính tăng thêm cho nhân vật khi cưỡi Bạch Hổ, Phi Kiếm,...
    *   Quản lý việc triệu hồi và chiến đấu của thú nuôi.
