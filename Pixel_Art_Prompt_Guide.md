# Hướng Dẫn Prompt AI Tạo Đồ Họa & Pipeline Xử Lý Sprite Sheet (J2ME Retro Style)

Tài liệu này định nghĩa tiêu chuẩn prompt AI và quy trình xử lý tự động (automated pipeline) cho đồ họa nhân vật của dự án **Tây Du Ký 2D**. Bằng cách kết hợp giữa Prompt AI chuẩn hóa và Code hậu xử lý, dự án đạt được sự đồng bộ 100% về kích thước, tỷ lệ và chất lượng hình ảnh của các nhân vật mới.

---

## 1. Tiêu Chuẩn Tạo Ảnh Chân Dung Nhân Vật (UI Portrait / Avatar)

Dùng cho giao diện chọn hệ phái (Character Creation) và thanh thông tin nhân vật (HP/MP HUD ở góc trên bên trái).

### Mẫu Prompt Tổng Quát
> **Prompt:** `A high-quality 2D pixel art character portrait of a [Faction] [Gender], [Weapon], J2ME retro mobile game style, vibrant colors, 16-bit color palette, detailed face and armor, solid dark background, clean pixel outline, fantasy RPG game asset.`
>
> **Negative Prompt:** `gradient background, realistic, 3D render, photographic, text, watermark, blurry, low resolution, noise, messy pixels, border frame.`

### Các Ví Dụ Thực Tế Theo Môn Phái:
*   **Thần Tộc (Kiếm Tiên):**
    > `A high-quality 2D pixel art character portrait of a divine Taoist immortal swordsman (Thần Tộc), male, holding a glowing crystal sword, J2ME retro mobile game style, vibrant colors, 16-bit color palette, clean pixel outline, solid dark background, fantasy RPG.`
*   **Ma Tộc (Ma Đao / Sát Thủ):**
    > `A high-quality 2D pixel art character portrait of a dark demon rogue girl (Ma Tộc), female, holding dual daggers, dark purple and red clothing, shadow aura, J2ME retro mobile game style, 16-bit color palette, solid dark background.`
*   **Yêu Tộc (Yêu Vương):**
    > `A high-quality 2D pixel art character portrait of a brute beast king warrior (Yêu Tộc), male, tiger features, wearing heavy bronze armor, J2ME retro mobile game style, 16-bit color palette, solid dark background.`

---

## 2. Tiêu Chuẩn Thiết Kế Sprite Sheet 4x4 Grid Standard

Để tự động hóa hoàn toàn việc cắt ảnh (Slicing) bằng code và loại bỏ hiện tượng nhân vật bị rung/lệch tâm khi di chuyển, tất cả Sprite Sheet nhân vật được quy định theo **Chuẩn Lưới 4x4 (16 Khung Hình)**:

| Thông số | Tiêu chuẩn | Ghi chú |
| :--- | :--- | :--- |
| **Độ phân giải tổng** | `1024 x 1024` px | Độ phân giải tối ưu cho AI và Import Unity |
| **Bố cục lưới (Grid)** | `4 cột x 4 dòng` | 16 khung hình chuyển động hoàn chỉnh |
| **Kích thước ô (Cell Size)** | `256 x 256` px | Tỷ lệ hình vuông hoàn hảo giữ nguyên cơ thể nhân vật |

### Quy Ước Hướng Di Chuyển Theo Dòng (Y-Axis)
*   **Dòng 0 (Dưới cùng - y từ 0 đến 256):** `Facing Up` (Quay lưng đi lên).
*   **Dòng 1 (y từ 256 đến 512):** `Facing Right` (Quay mặt sang phải).
*   **Dòng 2 (y từ 512 đến 768):** `Facing Left` (Quay mặt sang trái).
*   **Dòng 3 (Trên cùng - y từ 768 đến 1024):** `Facing Down` (Quay mặt đi xuống / Trực diện).

### Quy Ước Khung Hình Theo Cột (X-Axis)
*   **Cột 0:** Đứng yên (Idle).
*   **Cột 1:** Bước chân trái lên trước (Walk Left Step).
*   **Cột 2:** Đứng yên (Idle).
*   **Cột 3:** Bước chân phải lên trước (Walk Right Step).

### Mẫu Prompt Sinh Sprite Sheet Bằng AI (Midjourney & Stable Diffusion)
> **Prompt:** `A pixel art sprite sheet of a mini chibi [Character Description], walking animation cycle, arranged in a perfect 4x4 grid of 16 frames, total resolution 1024x1024, each frame size 256x256, 4 rows of directions (row 1 top: facing down, row 2: facing left, row 3: facing right, row 4 bottom: facing up), J2ME 16-bit retro game style, clean solid background, clean sharp pixel lines.`
>
> **Negative Prompt:** `gradient background, isometric, 3D, shadows, blurry, uneven grid, messy pixels, merge frames, texts, watermark.`

---

## 3. Quy Trình Hậu Xử Lý Tự Động (Automated Ingestion Pipeline)

AI tạo ảnh đôi khi sẽ gặp lỗi: lệch vị trí giữa các ô, viền răng cưa mờ (semi-transparent), hoặc nền không hoàn toàn trong suốt. Chúng ta sử dụng bộ công cụ tự động hóa đã viết để chuẩn hóa ảnh trước khi đưa vào Unity.

### Tập Lệnh Sử Dụng:
1.  **[process_spritesheet.py](file:///d:/AgentAI/TayDuKy2D/utils/process_spritesheet.py) (Python):** Thực hiện loại bỏ màu nền, xóa viền mờ (fringe pixels), tự động căn giữa ngang (horizontal center), và căn chỉnh lề dưới dọc (vertical bottom-align) để chân nhân vật luôn chạm đất đồng nhất.
2.  **[process_character.ps1](file:///d:/AgentAI/TayDuKy2D/utils/process_character.ps1) (PowerShell):** Pipeline 1-click chạy tập lệnh Python trên, sao chép kết quả vào thư mục Assets của Unity, và tự động tạo/sửa file `.meta` tương ứng để cắt grid 4x4 mà không cần mở Unity Editor.

### Cách Thực Hiện Nhập Nhân Vật Mới (1-Click):
Khi có một file ảnh Sprite Sheet thô (ví dụ: `duong_tang_raw.png`), bạn chỉ cần chạy lệnh PowerShell sau:

```powershell
powershell -File utils/process_character.ps1 -CharacterName "duong_tang_sheet" -RawImagePath "đường_dẫn_tới/duong_tang_raw.png"
```

### Các Tham Số Tùy Chọn Của Python Processor:
Nếu muốn tinh chỉnh sâu hơn, bạn có thể chạy trực tiếp file Python:
*   `--input`: Đường dẫn ảnh thô.
*   `--output`: Đường dẫn lưu ảnh đã xử lý.
*   `--detect-bg`: Tự động nhận diện màu nền từ pixel góc trên bên trái (mặc định bật).
*   `--bg-color`: Chỉ định màu nền cụ thể cần xóa (Ví dụ: `255,255,255` hoặc `#ffffff`).
*   `--tolerance`: Độ nhạy khi xóa nền (mặc định `30`, tăng lên nếu nền có hạt nhiễu).
*   `--align-y`: Hướng căn chỉnh dọc (`bottom`, `center`, `top`). Khuyên dùng `bottom` cho nhân vật di chuyển.
*   `--bottom-margin`: Khoảng cách chân nhân vật tới đáy ô (mặc định `24` pixel để nhân vật đứng cân đối trên tilemap).
*   `--add-outline`: Thêm đường viền đen 1-pixel bao quanh nhân vật để làm nổi bật phong cách J2ME retro game.

---

## 4. Tích Hợp Vào Cảnh Unity (Assembling)

Sau khi chạy tập lệnh import:
1.  File ảnh đã xử lý trong suốt và file `.meta` đi kèm sẽ xuất hiện tại `Assets/Sprites/Characters/`.
2.  Mở Unity Editor hoặc chạy script lắp ráp tự động để Unity ghi nhận thay đổi:
    *   Tập lệnh [SetupTestScene.cs](file:///d:/AgentAI/TayDuKy2D/tayduky-client/Assets/Editor/SetupTestScene.cs) sẽ tự động nạp các lát cắt này dựa trên tên đã cắt trong `.meta` (ví dụ: `duong_tang_sheet_down_0`).

---

## 5. Tiêu Chuẩn Thiết Kế & Tạo Ảnh Nền Bản Đồ (Map Backgrounds & Portals)

Để đảm bảo các bản đồ (Map) được AI sinh ra có cấu trúc nhất quán, hiển thị rõ vị trí cổng dịch chuyển và hỗ trợ kế thừa dễ dàng cho các map tiếp theo, chúng ta tuân thủ quy tắc sau:

### Quy Tắc Thiết Kế Bản Đồ
1. **Kích thước bản đồ tiêu chuẩn:** `1024 x 1024` px (Tương đương lưới 24x24 ô trong Unity, tỷ lệ co giãn tự động được MapManager xử lý).
2. **Góc nhìn (Perspective):** Góc nhìn từ trên xuống (Top-down view hoặc high-angle top-down) phù hợp với thể loại game nhập vai 2D cổ điển (retro 16-bit J2ME style).
3. **Vị trí Cổng Dịch Chuyển (Portals):**
   - Mỗi map phải có tối thiểu **1 cổng dịch chuyển**.
   - Thiết kế cổng dịch chuyển dưới dạng một cổng đá cổ kính với vòng xoáy năng lượng huyền ảo (ví dụ: vòng xoáy ma pháp xanh lam phát sáng).
   - Tọa độ cổng dịch chuyển được cấu hình tĩnh trong `maps.json` cùng với thời gian đứng chờ (`stand_time`) và khoảng cách Chebyshev kích hoạt (`trigger_distance`).
   - Hình ảnh cổng trên bản đồ nền cần được vẽ đúng vị trí tương đối (ví dụ: ở mép sát trên hoặc mép sát dưới chính giữa bản đồ).

### Các Mẫu Prompt Tạo Bản Đồ (Map Prompts)

Các prompt sau được dùng để sinh ảnh nền bản đồ thông qua AI:

*   **Hội Bàn Đào (Map 101 - Vườn đào tiên):**
    > **Prompt:** `A high-quality 2D pixel art top-down map of a celestial peach garden (Hội Bàn Đào), J2ME retro mobile game style, vibrant colors, 16-bit color palette. Soft green grass, blooming pink peach trees, celestial misty clouds. At the top-center edge, there is an ancient stone portal archway with a glowing blue magical energy vortex. Top-down view suitable for RPG game map, clean pixel texture.`
    >
    > **Negative Prompt:** `gradient background, isometric, 3D, shadows, blurry, uneven grid, messy pixels, texts, watermark, character, player, animal.`

*   **Dao Trì (Map 102 - Sân cẩm thạch trắng):**
    > **Prompt:** `A high-quality 2D pixel art top-down map of a celestial marble courtyard (Dao Trì), J2ME retro mobile game style, vibrant colors, 16-bit color palette. White marble tiles, floating clouds, gold and jade stone pillars. At the bottom-center edge, there is an ancient stone portal archway with a glowing blue magical energy vortex. Top-down view suitable for RPG game map, clean pixel texture.`
    >
    > **Negative Prompt:** `gradient background, isometric, 3D, shadows, blurry, uneven grid, messy pixels, texts, watermark, character, player, animal.`

Nhờ lưu trữ các mẫu prompt này, các lập trình viên hoặc AI tiếp theo có thể dễ dàng nhân bản/kế thừa phong cách đồ họa J2ME retro 16-bit để thiết kế các map sau (ví dụ: Trường An, Cao Lão Trang, Hoa Quả Sơn...) một cách thống nhất.

