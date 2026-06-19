package gateway

import (
	"encoding/json"
	"fmt"
	"net"
	"strings"
	"sync"
	"time"

	"tayduky-server/internal/gameplay/combat"
	"tayduky-server/internal/gameplay/move"
	"tayduky-server/internal/models"
)

// ============================================================
// In-Memory State (mirrors server.py dicts for Phase 1)
// ============================================================

var (
	// accountsDB: username -> password (plain text cho prototype; sẽ thay bằng bcrypt + PostgreSQL sau)
	accountsDB = map[string]string{
		"admin":   "admin",
		"shinichi": "123",
	}

	// accountCharacters: username -> characterID
	accountCharacters = map[string]int{
		"admin":   1024,
		"shinichi": 1024,
	}

	// playerStatsMap: characterID -> PlayerStats
	playerStatsMap = map[int]*models.PlayerStats{
		1024: {
			CharacterID: 1024, AccountID: 1, Name: "shinichi",
			Level: 32, HP: 100, HPMax: 100, MP: 50, MPMax: 50,
			Faction: "Thần Tộc", CharClass: "Pháp Sư", Gender: "Nam",
			Attack: 25, Defense: 10, CritRate: 0.15, Gold: 0, Exp: 0,
			MapID: 101, PosX: 12, PosY: 5,
		},
	}

	// monsterStats: monsterID -> Entity (for combat)
	monsterStats = map[int]*combat.Entity{
		999: {ID: 999, Name: "Tiểu Toàn Phong", Level: 15, Faction: "Yêu Tộc",
			Attack: 12, Defense: 5, HPMax: 60, HPCurrent: 60, CritRate: 0.05},
	}

	// characterQuests: charID -> questID -> {status, progress}
	characterQuests = map[int]map[int]*QuestProgress{}
	questMu         sync.RWMutex

	statsMu sync.RWMutex
	acctMu  sync.RWMutex
	nextID  = 1025
	idMu    sync.Mutex
)

// QuestProgress lưu tiến trình nhiệm vụ của nhân vật.
type QuestProgress struct {
	Status   string
	Progress int
}

// ============================================================
// TCP Dispatcher — Xử lý từng kết nối trong goroutine riêng
// ============================================================

// HandleTCPClient là entry point cho mỗi kết nối TCP mới.
// Phản chiếu hàm handle_client() trong server.py.
func HandleTCPClient(conn net.Conn, manager *ClientManager) {
	client := manager.Register(conn)
	defer func() {
		manager.Unregister(conn)
		conn.Close()
		fmt.Printf("[TCP] Client disconnected: %s\n", conn.RemoteAddr())
	}()

	buf := make([]byte, 4096)
	for {
		n, err := conn.Read(buf)
		if err != nil {
			break
		}

		rawPayload := strings.TrimSpace(string(buf[:n]))
		fmt.Printf("[RECEIVED] From %s: %s\n", conn.RemoteAddr(), rawPayload)

		// Detect action_id
		var base models.BasePacket
		if err := json.Unmarshal([]byte(rawPayload), &base); err != nil {
			fmt.Printf("[ERROR] Invalid JSON from %s: %v\n", conn.RemoteAddr(), err)
			continue
		}

		// Dispatch
		switch base.ActionID {
		case 1000:
			handleLogin(rawPayload, conn, manager)
		case 1001:
			handleMove(rawPayload, conn, client, manager)
		case 1002:
			handleCombat(rawPayload, conn)
		case 1003:
			handleCreateCharacter(rawPayload, conn)
		case 1004:
			handleRegister(rawPayload, conn)
		case 1005:
			handleChat(rawPayload, conn, manager)
		case 1008:
			handleQuest(rawPayload, conn)
		case 1010:
			handleMount(rawPayload)
		case 1011:
			handlePet(rawPayload)
		default:
			fmt.Printf("[WARNING] Unknown action_id: %d\n", base.ActionID)
		}
	}
}

// ============================================================
// Handlers — Phản chiếu 1:1 với các hàm handle_* trong server.py
// ============================================================

func sendJSON(conn net.Conn, payload interface{}) {
	data, err := json.Marshal(payload)
	if err != nil {
		fmt.Printf("[ERROR] Failed to marshal response: %v\n", err)
		return
	}
	conn.Write(data)
}

// handleLogin — action_id: 1000
func handleLogin(raw string, conn net.Conn, manager *ClientManager) {
	var pkt models.LoginPacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}
	fmt.Printf("[LOGIN REQUEST] User=%s\n", pkt.Username)

	acctMu.RLock()
	pwd, exists := accountsDB[pkt.Username]
	charID, hasChar := accountCharacters[pkt.Username]
	acctMu.RUnlock()

	if !exists || pwd != pkt.Password {
		sendJSON(conn, models.LoginResponse{
			ActionID: 2000, Status: "fail",
			Message: "Sai tài khoản hoặc mật khẩu!",
		})
		return
	}

	if hasChar {
		statsMu.RLock()
		stats := playerStatsMap[charID]
		statsMu.RUnlock()

		// Cập nhật MapID của Client sau khi login thành công
		if client, ok := manager.GetClient(conn); ok {
			client.CharacterID = charID
			client.MapID = stats.MapID
		}

		sendJSON(conn, models.LoginResponse{
			ActionID: 2000, Status: "success", HasCharacter: true,
			CharacterID: charID, Name: stats.Name, Level: stats.Level,
			HP: stats.HP, HPMax: stats.HPMax, Faction: stats.Faction,
			CharClass: stats.CharClass, Gender: stats.Gender,
		})
	} else {
		sendJSON(conn, models.LoginResponse{
			ActionID: 2000, Status: "success", HasCharacter: false,
		})
	}
}

// handleRegister — action_id: 1004
func handleRegister(raw string, conn net.Conn) {
	var pkt models.RegisterPacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}
	fmt.Printf("[REGISTER REQUEST] User=%s\n", pkt.Username)

	acctMu.Lock()
	_, exists := accountsDB[pkt.Username]
	if exists {
		acctMu.Unlock()
		sendJSON(conn, models.LoginResponse{
			ActionID: 2000, Status: "fail",
			Message: "Tài khoản này đã tồn tại!",
		})
		return
	}
	accountsDB[pkt.Username] = pkt.Password
	acctMu.Unlock()

	fmt.Printf("[REGISTER SUCCESS] New account: %s\n", pkt.Username)
	sendJSON(conn, models.LoginResponse{
		ActionID: 2000, Status: "success", HasCharacter: false,
	})
}

// handleCreateCharacter — action_id: 1003
func handleCreateCharacter(raw string, conn net.Conn) {
	var pkt models.CreateCharacterPacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}
	fmt.Printf("[CREATE CHAR] User=%s, Name=%s, Faction=%s, Class=%s\n",
		pkt.Username, pkt.Name, pkt.Faction, pkt.CharClass)

	// Kiểm tra tên trùng
	statsMu.RLock()
	for _, s := range playerStatsMap {
		if strings.EqualFold(s.Name, pkt.Name) {
			statsMu.RUnlock()
			sendJSON(conn, models.CreateCharacterResponse{
				ActionID: 2003, Status: "fail", Message: "Tên nhân vật đã tồn tại!",
			})
			return
		}
	}
	statsMu.RUnlock()

	// Lấy chỉ số base theo lớp
	cls := models.GetClassStats(pkt.CharClass)

	idMu.Lock()
	charID := nextID
	nextID++
	idMu.Unlock()

	newStats := &models.PlayerStats{
		CharacterID: charID, Name: pkt.Name,
		Level: 1, HP: cls.HPBase, HPMax: cls.HPBase,
		MP: cls.MPBase, MPMax: cls.MPBase,
		Faction: pkt.Faction, CharClass: pkt.CharClass, Gender: pkt.Gender,
		Attack: cls.AttackBase, Defense: cls.DefenseBase, CritRate: cls.CritRate,
		Gold: 0, Exp: 0, MapID: 101, PosX: 12, PosY: 8,
	}

	statsMu.Lock()
	playerStatsMap[charID] = newStats
	statsMu.Unlock()

	acctMu.Lock()
	accountCharacters[pkt.Username] = charID
	acctMu.Unlock()

	fmt.Printf("[CREATE CHAR SUCCESS] %s (ID: %d, Class: %s)\n", pkt.Name, charID, pkt.CharClass)
	sendJSON(conn, models.CreateCharacterResponse{
		ActionID: 2003, Status: "success",
		CharacterID: charID, Name: pkt.Name, Level: 1,
		HP: cls.HPBase, HPMax: cls.HPBase,
		Faction: pkt.Faction, CharClass: pkt.CharClass, Gender: pkt.Gender,
	})
}

// handleMove — action_id: 1001
func handleMove(raw string, conn net.Conn, client *Client, manager *ClientManager) {
	var pkt models.MovePacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}

	// Delegate validation đến move.go (kiểm tra speed hack, boundaries)
	ok, msg := move.HandleMoveRequest(pkt)
	if !ok {
		fmt.Printf("[MOVE BLOCKED] CharID=%d — %s\n", pkt.CharacterID, msg)
		return
	}

	// Cập nhật vị trí trên in-memory client
	client.MapID = pkt.MapID
	client.PosX = pkt.TargetX
	client.PosY = pkt.TargetY

	// Cập nhật in-memory player stats
	statsMu.Lock()
	if stats, ok := playerStatsMap[pkt.CharacterID]; ok {
		stats.MapID = pkt.MapID
		stats.PosX = pkt.TargetX
		stats.PosY = pkt.TargetY
	}
	statsMu.Unlock()

	// Lấy tên nhân vật để broadcast
	statsMu.RLock()
	name := "Unknown"
	mountType := "Chân chạy"
	if stats, ok := playerStatsMap[pkt.CharacterID]; ok {
		name = stats.Name
		if pkt.IsRiding {
			mountType = stats.MountType
		}
	}
	statsMu.RUnlock()

	fmt.Printf("[MOVE] CharID=%d moved on Map %d to (%d, %d)\n",
		pkt.CharacterID, pkt.MapID, pkt.TargetX, pkt.TargetY)

	// Broadcast vị trí mới cho người chơi xung quanh (AOI action_id: 2001)
	broadcastData, _ := json.Marshal(models.MoveEventResponse{
		ActionID: 2001, CharacterID: pkt.CharacterID, MapID: pkt.MapID,
		Name: name, CurrentX: pkt.TargetX, CurrentY: pkt.TargetY,
		Direction: pkt.Direction, MountType: mountType,
		Timestamp: time.Now().Unix(),
	})
	manager.Broadcast(broadcastData, conn, pkt.MapID)
}

// handleCombat — action_id: 1002
func handleCombat(raw string, conn net.Conn) {
	var pkt models.CombatPacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}

	statsMu.RLock()
	attackerStats, aOk := playerStatsMap[pkt.AttackerID]
	statsMu.RUnlock()

	target, tOk := monsterStats[pkt.TargetID]

	if !aOk || !tOk {
		fmt.Printf("[COMBAT ERROR] Attacker=%d, Target=%d not found\n", pkt.AttackerID, pkt.TargetID)
		return
	}

	attacker := &combat.Entity{
		ID: pkt.AttackerID, Name: attackerStats.Name, Level: attackerStats.Level,
		Faction: attackerStats.Faction, Attack: attackerStats.Attack,
		Defense: attackerStats.Defense, HPMax: attackerStats.HPMax,
		HPCurrent: attackerStats.HP, CritRate: attackerStats.CritRate,
	}

	result := combat.ExecuteAttack(attacker, target)
	fmt.Printf("[COMBAT] %s hit %s for %d dmg (Crit=%v, Dead=%v)\n",
		attacker.Name, target.Name, result.Damage, result.IsCrit, result.IsDead)

	// Reset HP quái nếu chết
	if result.IsDead {
		target.HPCurrent = target.HPMax
	}

	sendJSON(conn, models.CombatResultResponse{
		ActionID: 2002, AttackerID: pkt.AttackerID, TargetID: pkt.TargetID,
		Damage: result.Damage, IsCrit: result.IsCrit, IsDead: result.IsDead,
	})
}

// handleChat — action_id: 1005
func handleChat(raw string, conn net.Conn, manager *ClientManager) {
	var pkt models.ChatPacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}
	fmt.Printf("[CHAT] [%s] %s: %s\n", pkt.ChatChannel, pkt.SenderName, pkt.Message)

	broadcastData, _ := json.Marshal(models.ChatBroadcastResponse{
		ActionID: 2005, SenderID: pkt.SenderID, SenderName: pkt.SenderName,
		ChatChannel: pkt.ChatChannel, Message: pkt.Message,
		Timestamp: time.Now().Unix(),
	})

	if pkt.ChatChannel == "WORLD" {
		manager.BroadcastAll(broadcastData)
	} else {
		// Faction/Guild broadcast: gửi đến cùng MapID (tạm thời)
		if client, ok := manager.GetClient(conn); ok {
			manager.Broadcast(broadcastData, nil, client.MapID)
		}
	}
}

// handleQuest — action_id: 1008
func handleQuest(raw string, conn net.Conn) {
	var pkt models.QuestPacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}
	fmt.Printf("[QUEST] CharID=%d, QuestID=%d, Status=%s, Progress=%d\n",
		pkt.CharacterID, pkt.QuestID, pkt.Status, pkt.ProgressCount)

	questMu.Lock()
	if _, ok := characterQuests[pkt.CharacterID]; !ok {
		characterQuests[pkt.CharacterID] = make(map[int]*QuestProgress)
	}
	charQuests := characterQuests[pkt.CharacterID]
	alreadyCompleted := false
	if prev, ok := charQuests[pkt.QuestID]; ok {
		alreadyCompleted = (prev.Status == "COMPLETED")
	}
	charQuests[pkt.QuestID] = &QuestProgress{Status: pkt.Status, Progress: pkt.ProgressCount}
	questMu.Unlock()

	statsMu.RLock()
	stats, statsOk := playerStatsMap[pkt.CharacterID]
	statsMu.RUnlock()

	if !statsOk {
		return
	}

	message := fmt.Sprintf("Tiến trình nhiệm vụ cập nhật (%d)", pkt.ProgressCount)

	if pkt.Status == "COMPLETED" && !alreadyCompleted {
		// Cấp thưởng (giá trị cố định tạm thời, sẽ load từ quests.json sau)
		expReward := 200
		goldReward := 100

		statsMu.Lock()
		stats.Gold += goldReward
		stats.Exp += expReward
		leveledUp := false
		for {
			needed := models.NextLevelExp(stats.Level)
			if stats.Exp >= needed {
				stats.Exp -= needed
				stats.Level++
				stats.HPMax = int(float64(stats.HPMax) * 1.1)
				stats.HP = stats.HPMax
				stats.Attack += 2
				stats.Defense++
				leveledUp = true
				fmt.Printf("[LEVEL UP] CharID=%d leveled up to %d!\n", pkt.CharacterID, stats.Level)
			} else {
				break
			}
		}
		statsMu.Unlock()

		message = fmt.Sprintf("Nhiệm vụ hoàn thành! Nhận: +%d EXP, +%d Vàng.", expReward, goldReward)
		if leveledUp {
			message += fmt.Sprintf(" Chúc mừng đã lên cấp %d!", stats.Level)
		}
	}

	sendJSON(conn, models.QuestSyncResponse{
		ActionID: 2008, QuestID: pkt.QuestID, Status: pkt.Status,
		Name: stats.Name, Level: stats.Level, HP: stats.HP, HPMax: stats.HPMax,
		Gold: stats.Gold, Message: message,
	})
}

// handleMount — action_id: 1010
func handleMount(raw string) {
	var pkt models.MountPacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}

	statsMu.Lock()
	if stats, ok := playerStatsMap[pkt.CharacterID]; ok {
		stats.IsRiding = pkt.IsEquipped
		if !pkt.IsEquipped {
			stats.MountType = ""
		}
	}
	statsMu.Unlock()

	fmt.Printf("[MOUNT] CharID=%d, MountID=%d, Equipped=%v\n",
		pkt.CharacterID, pkt.MountID, pkt.IsEquipped)
}

// handlePet — action_id: 1011
func handlePet(raw string) {
	var pkt models.PetPacket
	if err := json.Unmarshal([]byte(raw), &pkt); err != nil {
		return
	}
	fmt.Printf("[PET] CharID=%d, PetID=%d, Summoned=%v\n",
		pkt.CharacterID, pkt.PetID, pkt.IsSummoned)
}


