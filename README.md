# Bản thử nghiệm Idle Defense (Unity)

Bản prototype Unity đơn giản, thân thiện người mới: người chơi đứng giữa bản đồ, tự đánh kẻ địch gần nhất, sống sót qua các đợt sóng vô hạn, và tự động trang bị đồ rơi tốt hơn.

Dự án giữ nhẹ về đồ họa nhưng **code đã tách lớp** (Core / Application / Presentation + `MonoBehaviour` scene):
- Hình khối đơn giản cho đồ họa
- TextMeshPro cho giao diện
- Pool enemy (tái kích hoạt thay vì tạo/xóa liên tục)
- Loot qua **sự kiện** (`GameEventBus`), inventory/player state thuần trong Core

## Tóm tắt gameplay

- Người chơi đứng gần trung tâm bản đồ.
- Kẻ địch spawn từ rìa và tiến vào trong.
- Người chơi tự tấn công kẻ địch gần nhất trong tầm.
- Kẻ địch gây sát thương khi áp sát.
- Sóng càng về sau càng khó hơn.
- Sóng boss cứ **N** sóng một lần (mặc định **10**; chỉnh trong `WaveScalingConfigSO` hoặc fallback trên `WaveSpawner` / `GameManager`).
- Kẻ địch có thể rơi đồ.
- Đồ cộng Attack, HP và Tốc độ đánh.
- Đồ tốt hơn được trang bị tự động.

---

## Hiện trạng dự án & kiến trúc (theo code hiện tại)

Phần này mô tả **đúng thực tế** repo: lớp code, luồng chạy, và phụ thuộc bắt buộc. Hướng dẫn scene phía dưới bổ sung chi tiết thao tác Unity.

### Tổ chức lớp (layer)

| Lớp | Thư mục / ví dụ | Vai trò |
|-----|------------------|---------|
| **Core** | `Scripts/Core/` | Logic thuần dùng chung: stats, items, random, enemy state/math (không gắn `GameServices`). |
| **GameServices** | `Scripts/GameServices/` | Nhóm logic theo scene bootstrap: **CoreServices** (Player, Inventory, Combat), **Systems** (Loot, EventBus), **Config** (Scaling, Wave), **Presentation** (`GameServices`, wire SO). |
| **Presentation** | `Scripts/Presentation/`, `Scripts/ScriptableObjects/` | Unity: `UnityRandomSource`, các ScriptableObject cấu hình. |
| **MonoBehaviour gốc** | `Scripts/*.cs`, `Scripts/Enemy/` | `GameManager`, `PlayerController`, `EnemyController`, `WaveSpawner`, `LootSystem`, `InventorySystem`, v.v. |

**Không có** file `CombatSystem.cs`. Giao tranh dùng `CombatService` (`GameServices/CoreServices/Combat`) + `PlayerState`; `PlayerController` chỉ đọc/ghi qua `GameServices.Player`.

### `GameServices` — trung tâm scene

`GameServices` (`DefaultExecutionOrder(-500)`) là **điểm bắt buộc** cho luồng chính:

- Tạo runtime: `ScalingDefinition`, `WaveProgressionDefinition`, `LootDropService`, `InventoryState`, `PlayerState`, `CombatService`, `GameEventBus`.
- ScriptableObject tùy chọn: `PlayerBaseStatsSO`, `LootTableSO`, `EnemyScalingConfigSO`, `WaveScalingConfigSO`.
- `PlayerController`, `LootSystem`, `InventorySystem` đều cần reference tới `GameServices` (hoặc `FindObjectOfType` trong `Awake`).

Thiếu `GameServices` → `PlayerController` log lỗi và không chạy combat đúng; loot/inventory không có state chung.

### Luồng loot (sự kiện, không gọi trực tiếp từ enemy → UI)

1. Enemy chết → `LootOnDeath.PublishDeathEvent()`.
2. `GameEventBus` phát `EnemyDiedEvent` (vị trí, `IsBoss`, `WaveNumber`).
3. `LootSystem` đã đăng ký → gọi `LootDropService.TryDropLoot(...)`.
4. `LootDropService` roll tỷ lệ / độ hiếm / chỉ số (theo `LootTableDefinition`), tạo `ItemData`, `InventoryState.TryAutoEquip`.
5. Phát `LootDroppedEvent` → `LootSystem` cập nhật `lootText`.

Enemy **không** gọi `TryDropLoot` trực tiếp; mọi thứ đi qua bus để tách publisher / subscriber.

### Luồng sóng & độ khó

- `GameManager` giữ `currentWave` và các multiplier (công thức mũ từ `EnemyScalingDefinition`).
- `WaveSpawner` đọc `WaveProgressionDefinition` (từ `WaveScalingConfigSO` hoặc fallback Inspector).
- `EnemyController` + `EnemyCombatMath`: chỉ số cuối = cơ bản × multiplier global × multiplier boss (không nhân đôi curve per-wave trong combat math).

### ScriptableObject có trong project

| Asset (menu Create) | Nội dung chính |
|---------------------|----------------|
| `IdleDefense/Player Base Stats` | Chỉ số gốc player → `StatBlock`. |
| `IdleDefense/Loot Table` | Tỷ lệ rơi, tier độ hiếm có trọng số, dải chỉ số, `WaveScalingPerWave` cho **đồ**, trọng số điểm auto-equip. |
| `IdleDefense/Enemy Scaling` | Tăng trưởng mũ HP/damage/spawn + hệ số boss. |
| `IdleDefense/Wave Progression` | Số enemy mỗi sóng, chu kỳ boss, tùy chọn override thời gian spawn/giữa sóng. |

### Cây thư mục `Scripts/` (thực tế)

```
Scripts/
├── GameManager.cs, WaveSpawner.cs, EnemyPool.cs, CameraFollow.cs
├── PlayerController.cs, LootSystem.cs, InventorySystem.cs
├── EnemyController.cs
├── Enemy/          → LootOnDeath, Health, EnemyMovement, EnemyCombat
├── Core/           → Items, Stats, Random, Enemy, Math (shared domain)
├── GameServices/
│   ├── CoreServices/ → Combat/, Inventory/, Player/
│   ├── Systems/      → Loot/, EventBus/
│   ├── Config/       → Scaling/, Wave/
│   └── Presentation/ → GameServices.cs
├── Presentation/   → UnityRandomSource.cs
└── ScriptableObjects/ → LootTableSO, PlayerBaseStatsSO, EnemyScalingConfigSO, WaveScalingConfigSO
```

### `ItemData` & túi đồ (Core)

- `ItemData` (`Core/Items/ItemData.cs`): tên, tier, danh sách `StatModifier` (Attack, MaxHp, AttackSpeed, …), gom qua `StatAggregator`.
- `InventoryState`: ba slot (Weapon / Armor / Trinket), phân loại theo **tên** chứa `blade` / `core` / còn lại → trinket; điểm so sánh qua `GetEquipScore(EquipScoringWeights)` (mặc định tương đương tổng có trọng số, cấu hình được từ `LootTableDefinition` / `LootTableSO`).
- `InventorySystem` (MonoBehaviour): chỉ **UI** — subscribe `EquipmentChanged` trên `InventoryState` và gọi `GetEquippedSummary()`; **không** phải nơi định nghĩa `GetBonusAttack()` (bonus nằm ở `PlayerState` / chỉ số player).

---

## Các script chính (tham chiếu nhanh)

**Scene / gameplay:** `GameManager`, `WaveSpawner`, `EnemyPool`, `EnemyController`, `PlayerController`, `CameraFollow`.

**Dịch vụ & UI:** `GameServices/Presentation/GameServices`, `LootSystem`, `InventorySystem`.

**Enemy (component):** `Enemy/LootOnDeath`, `Enemy/Health`, `Enemy/EnemyMovement`, `Enemy/EnemyCombat`.

**GameServices (đáng nhớ):** `CoreServices/*` (`CombatService`, `PlayerState`, `InventoryState`), `Systems/Loot`, `Systems/EventBus`, `Config/Scaling`, `Config/Wave`; **Core** chung: `Core/Items/ItemData`, stats, v.v.

---

## Phiên bản Unity

Khuyến nghị:
- Unity 2022 LTS trở lên

TextMeshPro có sẵn khi cài Unity chuẩn.

---

## Thiết lập scene đầy đủ

Tạo project 3D Unity mới, sau đó copy script vào thư mục `Assets/Scripts/` của project (nếu chưa có).

**Thứ tự khuyến nghị:** tạo **`GameServices`** (mục dưới) **trước** `Player`, `LootSystem`, `InventorySystem` — các script này phụ thuộc state được khởi tạo trong `GameServices.Awake`.

### 1) Tạo mặt đất

1. Trong Hierarchy, chuột phải
2. Chọn `3D Object > Plane`
3. Đổi tên thành `Ground`
4. Đặt Transform:
   - Position: `0, 0, 0`
   - Scale: `5, 1, 5`

Tùy chọn:
- Gán material xám đơn giản

---

### 2) Tạo người chơi

1. Chuột phải trong Hierarchy
2. Chọn `3D Object > Capsule`
3. Đổi tên thành `Player`
4. Đặt Transform:
   - Position: `0, 1, 0`
   - Scale: `1, 1, 1`

Gắn script `PlayerController` lên object Player.

- Gán **`Services`** → `GameServices` (bắt buộc: `PlayerState` được tạo trong `GameServices`).

Chỉ số gốc player mặc định nằm trong `GameServices` (`PlayerBaseStatsSO` hoặc default code: MaxHp 100, Attack 10, …). Inspector `PlayerController` không thay thế toàn bộ `StatBlock` trừ khi bạn mở rộng code.

Nếu `PlayerController` có field UI, nối ở phần UI (`hpText`, `statsText`, `equippedText`).

`InventorySystem` là object riêng; player lắng nghe đổi đồ qua `GameServices.Inventory.EquipmentChanged` (đã nối trong code `PlayerController`).

---

### 3) Tạo camera chính

1. Chọn `Main Camera`
2. Reset Transform nếu cần
3. Thêm script `CameraFollow`
4. Gán transform của Player làm target

Gợi ý camera:
- Offset: `0, 12, -10`
- Smooth Speed: `5`
- Nghiêng xuống nhẹ để nhìn rõ sân

Nếu cần, xoay camera khoảng:
- X: `45`
- Y: `0`
- Z: `0`

---

### 4) Tạo game manager

1. Tạo object rỗng
2. Đổi tên thành `GameManager`
3. Thêm script `GameManager`

Object này quản lý:
- Trạng thái game
- Sóng hiện tại
- Scale độ khó
- Game over
- Cập nhật UI

---

### 4b) Tạo GameServices (bắt buộc cho player / loot / túi đồ)

1. Tạo object rỗng, đặt tên `GameServices`
2. Thêm script `GameServices` (namespace `IdleDefense.Presentation`)
3. (Tùy chọn) **Create → IdleDefense → …** để tạo asset và gán:
   - `Player Base Stats`
   - `Loot Table` (tỷ lệ rơi, tier độ hiếm, scale đồ theo sóng)
   - `Enemy Scaling`, `Wave Progression` (độ khó & sóng)

Không gán asset vẫn chạy được: code dùng `EnemyScalingDefinition` / `WaveProgressionDefinition` / `LootTableDefinition` mặc định.

`GameServices` tạo sẵn: `PlayerState`, `InventoryState`, `LootDropService`, `CombatService`, `GameEventBus`.

---

### 5) Tạo prefab enemy

Tạo enemy cơ bản trước:

1. Chuột phải trong Hierarchy
2. Chọn `3D Object > Sphere`
3. Đổi tên thành `Enemy`
4. Đặt Transform:
   - Position: `0, 0.5, 8`
   - Scale: `1, 1, 1`
5. Tag là `Enemy`

Thêm script `EnemyController`.

Gợi ý giá trị enemy nếu có trong Inspector:
- Tốc độ di chuyển: `2.5`
- Max HP: `20`
- Sát thương va chạm: `6`
- Chu kỳ tick sát thương: `1`

Kéo object vào thư mục `Prefabs` để tạo prefab.

Sau khi có prefab:
- Xóa bản copy trong scene nếu pool sẽ spawn từ prefab

---

### 6) Tạo enemy pool

1. Tạo object rỗng
2. Đổi tên thành `EnemyPool`
3. Thêm script `EnemyPool`
4. Gán prefab Enemy
5. Đặt kích thước pool ban đầu

Gợi ý:
- Initial Pool Size: `50` khi test
- Tăng lên `80` hoặc `100` nếu muốn sóng đông hơn

Pool giúp game dùng `SetActive` thay vì tạo/hủy enemy liên tục.

---

### 7) Tạo wave spawner

1. Tạo object rỗng
2. Đổi tên thành `WaveSpawner`
3. Thêm script `WaveSpawner`
4. Gán:
   - Player
   - EnemyPool
   - GameManager (nếu script cần)

Gợi ý giá trị:
- Bán kính spawn: `18`
- Thời gian giữa mỗi lần spawn: `0.75`
- Số enemy mỗi sóng: bắt đầu khoảng `8`
- Vị trí spawn nên quanh rìa map

Spawner cần:
- Spawn enemy từ ngoài / rìa
- Tăng **số lượng** enemy theo sóng (tuyến tính theo `WaveProgressionDefinition`)
- Spawn boss mỗi **N** sóng (mặc định 10; cấu hình qua `WaveScalingConfigSO` / fallback)

---

### 8) Tạo hệ thống loot

1. Tạo object rỗng
2. Đổi tên thành `LootSystem`
3. Thêm script `LootSystem`

Gán giá trị Inspector:

#### Tham chiếu
- Inventory System: kéo object `InventorySystem`
- Loot Text: TMP tùy chọn cho dòng loot vừa rơi

#### Gợi ý mặc định
- Tỷ lệ rơi enemy thường: `0.35`
- Tỷ lệ rơi boss: `1.0`

Chỉ số gốc của item:
- Attack Min: `1` / Max: `3`
- HP Min: `5` / Max: `12`
- Attack Speed Min: `0.03` / Max: `0.12`

Cách hoạt động (thực tế trong code):
- Enemy chết → `LootOnDeath` phát `EnemyDiedEvent` qua `GameServices.Events`
- `LootSystem` nhận sự kiện → `LootDropService.TryDropLoot(...)`
- Roll thành công → sinh `ItemData`, `InventoryState.TryAutoEquip`, phát `LootDroppedEvent` → cập nhật `lootText`

Gán **`Services`** → `GameServices` trên `LootSystem`.

---

### 9) Tạo hệ thống túi đồ

1. Tạo object rỗng
2. Đổi tên thành `InventorySystem`
3. Thêm script `InventorySystem`

Thiết lập ban đầu tùy chọn:
- Để slot trống
- Gán TMP cho tóm tắt túi đồ

Ô trang bị:
- Weapon Item
- Armor Item
- Trinket Item

Phân loại ô (trong `InventoryState.TryAutoEquip`, so khớp chữ thường):
- Tên chứa `blade` → Weapon
- `core` → Armor
- Còn lại (ví dụ `charm`) → Trinket

`InventorySystem` chỉ phục vụ UI: gọi `Services.Inventory.GetEquippedSummary()` khi đồ đổi.

Bonus chỉ số cho combat đi qua **`PlayerState`** / `StatBlock` (không phải API trên `InventorySystem`).

---

## UI với TextMeshPro

### 1) Tạo Canvas

1. Chuột phải Hierarchy
2. Chọn `UI > Canvas`
3. Nếu được hỏi, import TMP Essentials
4. Canvas:
   - Render Mode: `Screen Space - Overlay`

Đảm bảo scene có `EventSystem`.

---

### 2) Text sóng (Wave)

1. Chuột phải Canvas
2. Chọn `UI > Text - TextMeshPro`
3. Đổi tên `WaveText`
4. Nội dung: `Wave: 1`
5. Anchor góc trên-trái
6. Vị trí gợi ý:
   - X: `120`
   - Y: `-40`
7. Cỡ chữ: `28`

Gán object này vào:
- `GameManager.waveText`

---

### 3) Text trạng thái

1. Nhân đôi hoặc tạo TMP khác
2. Đổi tên `StateText`
3. Để trống nội dung
4. Anchor giữa-trên hoặc trên-giữa
5. Cỡ chữ: `32`

Gán vào:
- `GameManager.stateText`

Dùng cho:
- Game Over
- Thông báo trạng thái đơn giản khác

---

### 4) Text chỉ số player

1. Tạo TMP khác
2. Đổi tên `PlayerStatsText`
3. Anchor góc trên-phải
4. Kích thước gợi ý:
   - Rộng: `350`
   - Cao: `180`
5. Cỡ chữ: `24`

Gán vào `PlayerController` nếu script có field TMP cho chỉ số.

Ví dụ nội dung:
- HP
- Attack
- Tốc độ đánh
- Tầm

---

### 5) Text túi đồ

1. Tạo TMP khác
2. Đổi tên `InventoryText`
3. Anchor góc dưới-trái
4. Khung rộng hơn
5. Cỡ chữ: `20`
6. Bật word wrap nếu cần

Gán vào:
- `InventorySystem.inventoryText`

Hiển thị đồ đang mặc và tổng cộng chỉ số.

---

### 6) Text loot

1. Tạo TMP khác
2. Đổi tên `LootText`
3. Anchor dưới-giữa
4. Cỡ chữ: `22`
5. Mặc định ví dụ:
   - `Loot: None`

Gán vào:
- `LootSystem.lootText`

Hiển thị kết quả rơi đồ gần nhất.

---

## Checklist nối Inspector

Sau khi tạo object, nối reference cẩn thận.

### GameManager
Gán:
- Player → `Player`
- Wave Spawner → `WaveSpawner`
- Wave Text → `WaveText`
- State Text → `StateText`

### GameServices
Gán các ScriptableObject tùy chọn (Loot Table, Enemy Scaling, …).

### PlayerController
Gán:
- **Services** → `GameServices`
- TMP chỉ số / HP / equipped → các field tương ứng

### InventorySystem
Gán:
- **Services** → `GameServices`
- Inventory Text → `InventoryText`

### CameraFollow
Gán:
- Target → `Player`

### EnemyPool
Gán:
- Enemy Prefab → prefab enemy của bạn

### WaveSpawner
Gán:
- Player → `Player`
- Enemy Pool → `EnemyPool`

### LootSystem
Gán:
- **Services** → `GameServices`
- Loot Text → `LootText`

(Không cần kéo `InventorySystem` vào `LootSystem` — loot dùng `GameServices.Inventory` bên trong `LootDropService`.)

---

## Gợi ý hình ảnh đơn giản

Giữ giao diện dễ nhìn.

### Player
- Dùng Capsule
- Material màu xanh

### Enemy thường
- Dùng Sphere
- Material đỏ

### Boss
- Sphere lớn hơn hoặc Cube
- Đỏ đậm hoặc tím
- Scale khoảng `2, 2, 2`

### Mặt đất
- Plane màu trung tính

Có thể thêm object con phía trên enemy làm thanh HP đơn giản (tùy chọn).

---

## Quy tắc loot và item

### Tỷ lệ độ hiếm
- Thường: `70%`
- Hiếm: `25%`
- Sử thi: `5%`

### Chỉ số
Mỗi item có thể cộng:
- Attack
- HP
- Tốc độ đánh

### Tự trang bị
Điểm so sánh: `ItemData.GetEquipScore(EquipScoringWeights)` — mặc định tương đương tổng có trọng số Attack / MaxHp / AttackSpeed (cấu hình trong `LootTableDefinition` / `LootTableSO`).

Nếu điểm cao hơn đồ đang mặc cùng ô → thay tự động.

### Quy tắc ô
Tên sinh ra quyết định ô:
- `Blade` = Vũ khí
- `Core` = Giáp
- `Charm` = Phụ kiện

---

## Luồng gameplay ví dụ

1. Scene chạy; `GameServices.Awake` tạo state (player, inventory, loot service, bus sự kiện).
2. `GameManager` bắt đầu sóng 1
3. `WaveSpawner` spawn enemy từ rìa
4. Enemy tiến về player
5. `PlayerController` dùng `CombatService` + `PlayerState` để tự đánh enemy gần nhất trong tầm
6. Enemy chết → `LootOnDeath.PublishDeathEvent`
7. `EnemyDiedEvent` → `LootSystem` → `LootDropService.TryDropLoot(...)`
8. Nếu rơi đồ → sinh `ItemData`, `InventoryState.TryAutoEquip`
9. `LootDroppedEvent` → cập nhật UI loot; nếu đổi đồ → `EquipmentChanged` → player tính lại chỉ số
10. Hết sóng → `GameManager.AdvanceWave()` cập nhật multiplier độ khó

---

## Sóng và scale độ khó (cấu trúc)

Phần này mô tả **kiến trúc** sóng và độ khó: dữ liệu ở đâu, công thức, luồng runtime.

### Tổng quan

| Thành phần | Vai trò |
|------------|---------|
| `GameServices` | Khởi tạo scene: gán ScriptableObject, tạo `EnemyScalingDefinition` + `WaveProgressionDefinition` lúc chạy. |
| `EnemyScalingConfigSO` | Cấu hình **mũ (exponential)** cho HP / sát thương / tốc độ spawn theo sóng + hệ số boss (chỉ số). |
| `WaveScalingConfigSO` | Cấu hình **số enemy mỗi sóng**, **boss mỗi N sóng**, tùy chọn ghi đè delay spawn / nghỉ giữa sóng. |
| `GameManager` | `currentWave`, `enemyHealthMultiplier`, `enemyDamageMultiplier`, `spawnRateMultiplier` (tính lại mỗi sóng). |
| `WaveSpawner` | Đọc `WaveProgressionDefinition` (hoặc fallback Inspector), spawn lính + boss khi `IsBossWave()`. |
| `EnemyController` + `EnemyCombatMath` | `chỉ số gốc × hệ số GameManager × hệ số boss` (không nhân đôi đường cong theo sóng). |

### Công thức (hàm mũ)

Đặt \(n = \max(0, \text{currentWave} - 1)\).

- **Hệ số máu:** \(M_\text{HP} = r_\text{HP}^{\,n}\)
- **Hệ số sát thương:** \(M_\text{DMG} = r_\text{DMG}^{\,n}\)
- **Hệ số tốc độ spawn:** \(M_\text{spawn} = r_\text{spawn}^{\,n}\)

Mặc định trong `EnemyScalingDefinition` / asset: \(r_\text{HP} \approx 1.12\), \(r_\text{DMG} \approx 1.06\), \(r_\text{spawn} \approx 1.042\) (chỉnh trên ScriptableObject).

### Sóng boss

- Điều kiện: `BossWaveInterval > 0`, `SpawnBossOnBossWave == true`, và `currentWave % BossWaveInterval == 0`.
- `BossWaveInterval = 0` → không có sóng boss.
- Khi **không** gán `WaveScalingConfigSO` trên `GameServices`: dùng `bossWaveIntervalFallback` trên `WaveSpawner` (và `GameManager` nếu không có reference spawner).

Cấu trúc file chi tiết và cây thư mục đầy đủ nằm ở mục **「Hiện trạng dự án & kiến trúc」** phía trên (tránh trùng lặp).

### Thiết lập trong scene

1. Thêm GameObject `GameServices`, gán script `GameServices` (thứ tự chạy sớm).
2. **Create → IdleDefense → Enemy Scaling** → kéo asset vào ô `Enemy Scaling`.
3. **Create → IdleDefense → Wave Progression** → kéo asset vào ô `Wave Progression`.
4. `GameManager` và `WaveSpawner` lấy dữ liệu qua `GameServices.Instance` khi có SO; thiếu SO thì dùng field fallback trên `GameManager` / `WaveSpawner`.

### Fallback (không dùng ScriptableObject)

- `GameManager`: `healthGrowthPerWave`, `damageGrowthPerWave`, `spawnRateGrowthPerWave`, `bossWaveIntervalFallback`.
- `WaveSpawner`: `baseEnemiesPerWave`, `extraEnemiesPerWave`, `spawnBossEveryTenWaves`, `bossWaveIntervalFallback`.

---

## Gợi ý gỡ lỗi

### Enemy không bị đánh
Kiểm tra:
- Tag `Enemy`
- Object đang bật (active)
- Enemy trong tầm đánh
- `EnemyController.IsAlive()` hoạt động
- Tốc độ/timer đánh player không quá chậm

### Không bao giờ rơi loot
Kiểm tra:
- Có `GameServices` và `LootSystem.Services` đã gán
- Prefab enemy có `LootOnDeath` và `PublishDeathEvent` khi chết
- `LootSystem` đăng ký `EnemyDied` trên bus (component bật)
- Tỷ lệ rơi trong `LootTableSO` / default không quá thấp

### Text túi đồ không đổi
Kiểm tra:
- Đã gán `InventoryText`
- `InventorySystem.UpdateUI()` được gọi
- Tên item vẫn có `Blade`, `Core`, hoặc `Charm`

### Chỉ số cộng từ đồ không ảnh hưởng gameplay
Kiểm tra:
- `PlayerState` / `PlayerState.RecalculateHpAfterStatChange` khi đổi đồ (`OnEquipmentChanged` trong `PlayerController`)
- `PlayerController` dùng `Services.Player.GetAttack()`, `GetAttackSpeed()`, `GetFinalStats()` — không có `GetBonusAttack()` công khai trên `InventorySystem`

### Sóng không tăng
Kiểm tra:
- `WaveSpawner` nhận diện hết sóng
- `GameManager.AdvanceWave()` được gọi

---

## Gợi ý cân bằng mặc định

Giá trị an toàn cho lần test đầu.

### Player
- HP: `100`
- Attack: `10`
- Tốc độ đánh: `1.0`
- Tầm đánh: `6`

### Enemy
- HP: `20`
- Sát thương: `6`
- Tốc độ di chuyển: `2.5`

### Spawn
- Bán kính spawn: `18`
- Enemy mỗi sóng (ban đầu): `8`
- Delay giữa các lần spawn: `0.75`

### Độ khó (mũ)

Các multiplier runtime trên `GameManager` được tính từ `EnemyScalingConfigSO` (hoặc fallback Inspector). Không còn một tham số **tuyến tính** kiểu `Scaling Per Wave` đơn.

Khi chỉnh tay (không SO), trên `GameManager` có thể dùng:
- `healthGrowthPerWave`: ví dụ `1.12`
- `damageGrowthPerWave`: ví dụ `1.06`
- `spawnRateGrowthPerWave`: ví dụ `1.042`

Sóng 1 thường thấy các hệ số Health / Damage / Spawn là `1`.

### Loot
- Tỷ lệ rơi thường: `0.35`
- Tỷ lệ rơi boss: `1.0`

---

## Build game cho PC

### Windows

1. Mở Unity
2. Vào `File > Build Settings`
3. Chọn `PC, Mac & Linux Standalone`
4. Chọn:
   - Target Platform: `Windows`
   - Architecture: `x86_64`
5. Bấm `Add Open Scenes`
6. Bấm `Player Settings` nếu muốn đổi:
   - Tên sản phẩm
   - Tên công ty
   - Độ phân giải
   - Icon
7. Bấm `Build`
8. Chọn thư mục, ví dụ:
   - `Builds/Windows`
9. Đợi Unity build xong
10. Chạy file `.exe` vừa tạo

### Mac

1. Mở Unity
2. Vào `File > Build Settings`
3. Chọn `PC, Mac & Linux Standalone`
4. Chọn:
   - Target Platform: `Mac`
5. Bấm `Add Open Scenes`
6. Mở `Player Settings` nếu cần
7. Bấm `Build`
8. Chọn thư mục, ví dụ:
   - `Builds/Mac`
9. Đợi Unity export
10. Chạy bản build trên máy Mac

Lưu ý:
- Build Mac từ Windows có thể cần module nền tảng bổ sung trong Unity Hub.
- macOS mới có thể phải cho phép app trong cài đặt bảo mật.

---

## Gợi ý cấu trúc Hierarchy

Ví dụ hierarchy đơn giản:

- `Main Camera`
- `Directional Light`
- `Ground`
- `Player`
- `GameManager`
- `GameServices` (**nên có**; gán `LootTableSO`, `EnemyScalingConfigSO`, `WaveScalingConfigSO`, `PlayerBaseStatsSO` nếu dùng asset)
- `WaveSpawner`
- `EnemyPool`
- `LootSystem`
- `InventorySystem`
- `Canvas`
  - `WaveText`
  - `StateText`
  - `PlayerStatsText`
  - `InventoryText`
  - `LootText`

---

## Ghi chú tích hợp

- **Inventory UI:** `InventorySystem` → `Services.Inventory.GetEquippedSummary()` (và subscribe `EquipmentChanged` đã có trong script).
- **Chỉ số combat:** `PlayerState` qua `GameServices.Player` — `GetAttack()`, `GetMaxHp()`, `GetAttackSpeed()`, `GetFinalStats()`, v.v.
- **Loot:** Không gọi `TryDropLoot` từ prefab enemy trực tiếp; chuẩn là `LootOnDeath` → `EnemyDiedEvent` → `LootDropService.TryDropLoot`. Có thể gọi `TryDropLoot` thủ công từ code khác nếu truyền đủ `InventoryState` và `GameEventBus`.

Hệ **độ khó đồ** (loot) vẫn dùng **tuyến tính** `WaveScalingPerWave` trong `LootTableDefinition` — khác với **độ khó enemy** (mũ) trong `EnemyScalingDefinition`. Hai hệ độc lập trong cấu hình.
