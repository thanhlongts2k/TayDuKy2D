package models

import "time"

// Account đại diện cho một tài khoản người dùng trong hệ thống.
type Account struct {
	ID           int       `json:"id"`
	Username     string    `json:"username"`
	PasswordHash string    `json:"-"` // Never expose in JSON
	Email        string    `json:"email,omitempty"`
	Status       int       `json:"status"` // 1: Active, 0: Banned
	CreatedAt    time.Time `json:"created_at"`
}

// PlayerStats lưu trữ trạng thái in-memory của người chơi đang online.
// Được dùng trong ClientManager thay vì truy vấn DB liên tục.
type PlayerStats struct {
	CharacterID int    `json:"character_id"`
	AccountID   int    `json:"account_id"`
	Name        string `json:"name"`
	Level       int    `json:"level"`
	HP          int    `json:"hp"`
	HPMax       int    `json:"hp_max"`
	MP          int    `json:"mp"`
	MPMax       int    `json:"mp_max"`
	Faction     string `json:"faction"`
	CharClass   string `json:"class"`
	Gender      string `json:"gender"`
	Attack      int    `json:"attack"`
	Defense     int    `json:"defense"`
	CritRate    float64 `json:"crit_rate"`
	Gold        int    `json:"gold"`
	Exp         int    `json:"exp"`
	MapID       int    `json:"map_id"`
	PosX        int    `json:"pos_x"`
	PosY        int    `json:"pos_y"`
	IsRiding    bool   `json:"is_riding"`
	MountType   string `json:"mount_type"`
}

// CharacterClass chứa các thống số base cho mỗi lớp nhân vật khi tạo mới.
type CharacterClass struct {
	HPBase      int
	MPBase      int
	AttackBase  int
	DefenseBase int
	CritRate    float64
}

// GetClassStats trả về chỉ số cơ bản tương ứng với lớp nhân vật.
// Phản chiếu logic trong server.py handle_create_character().
func GetClassStats(charClass string) CharacterClass {
	switch charClass {
	case "Kiếm Tiên":
		return CharacterClass{HPBase: 120, MPBase: 50, AttackBase: 22, DefenseBase: 12, CritRate: 0.12}
	case "Pháp Sư":
		return CharacterClass{HPBase: 90, MPBase: 80, AttackBase: 28, DefenseBase: 8, CritRate: 0.15}
	case "Ma Đao":
		return CharacterClass{HPBase: 130, MPBase: 40, AttackBase: 22, DefenseBase: 12, CritRate: 0.20}
	case "Sát Thủ":
		return CharacterClass{HPBase: 110, MPBase: 45, AttackBase: 24, DefenseBase: 9, CritRate: 0.25}
	case "Yêu Vương":
		return CharacterClass{HPBase: 150, MPBase: 60, AttackBase: 19, DefenseBase: 16, CritRate: 0.08}
	case "Yêu Pháp":
		return CharacterClass{HPBase: 120, MPBase: 70, AttackBase: 21, DefenseBase: 11, CritRate: 0.12}
	default:
		return CharacterClass{HPBase: 100, MPBase: 50, AttackBase: 20, DefenseBase: 10, CritRate: 0.10}
	}
}

// NextLevelExp tính EXP cần thiết để lên cấp tiếp theo.
// Công thức: level * 120 (giống server.py)
func NextLevelExp(currentLevel int) int {
	return currentLevel * 120
}
