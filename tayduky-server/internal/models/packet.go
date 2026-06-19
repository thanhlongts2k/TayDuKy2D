package models

// ============================================================
// PACKET DEFINITIONS — Tây Du Ký Mobile Server
// Mọi gói tin client <-> server đều tuân theo cấu trúc JSON này.
// ============================================================

// BasePacket dùng để detect action_id trước khi deserialize chi tiết.
type BasePacket struct {
	ActionID int `json:"action_id"`
}

// ============================================================
// CLIENT -> SERVER PACKETS (action_id 1xxx)
// ============================================================

// LoginPacket — action_id: 1000
type LoginPacket struct {
	ActionID int    `json:"action_id"`
	Username string `json:"username"`
	Password string `json:"password"`
}

// MovePacket — action_id: 1001 (đã khai báo trong character.go, giữ nguyên)

// CombatPacket — action_id: 1002
type CombatPacket struct {
	ActionID   int `json:"action_id"`
	AttackerID int `json:"attacker_id"`
	TargetID   int `json:"target_id"`
}

// CreateCharacterPacket — action_id: 1003
type CreateCharacterPacket struct {
	ActionID    int    `json:"action_id"`
	Username    string `json:"username"`
	Name        string `json:"name"`
	Faction     string `json:"faction"`
	CharClass   string `json:"class"`
	Gender      string `json:"gender"`
}

// RegisterPacket — action_id: 1004
type RegisterPacket struct {
	ActionID int    `json:"action_id"`
	Username string `json:"username"`
	Password string `json:"password"`
}

// ChatPacket — action_id: 1005
type ChatPacket struct {
	ActionID    int    `json:"action_id"`
	SenderID    int    `json:"sender_id"`
	SenderName  string `json:"sender_name"`
	ChatChannel string `json:"chat_channel"` // "WORLD", "FACTION", "GUILD", "WHISPER"
	Message     string `json:"message"`
}

// QuestPacket — action_id: 1008
type QuestPacket struct {
	ActionID      int    `json:"action_id"`
	CharacterID   int    `json:"character_id"`
	QuestID       int    `json:"quest_id"`
	Status        string `json:"status"` // "IN_PROGRESS", "COMPLETED"
	ProgressCount int    `json:"progress_count"`
}

// MountPacket — action_id: 1010
type MountPacket struct {
	ActionID    int  `json:"action_id"`
	CharacterID int  `json:"character_id"`
	MountID     int  `json:"mount_id"`
	IsEquipped  bool `json:"is_equipped"`
}

// PetPacket — action_id: 1011
type PetPacket struct {
	ActionID    int  `json:"action_id"`
	CharacterID int  `json:"character_id"`
	PetID       int  `json:"pet_id"`
	IsSummoned  bool `json:"is_summoned"`
}

// ============================================================
// SERVER -> CLIENT PACKETS (action_id 2xxx)
// ============================================================

// LoginResponse — action_id: 2000
type LoginResponse struct {
	ActionID     int    `json:"action_id"`
	Status       string `json:"status"` // "success" | "fail"
	HasCharacter bool   `json:"has_character"`
	CharacterID  int    `json:"character_id,omitempty"`
	Name         string `json:"name,omitempty"`
	Level        int    `json:"level,omitempty"`
	HP           int    `json:"hp,omitempty"`
	HPMax        int    `json:"hp_max,omitempty"`
	Faction      string `json:"faction,omitempty"`
	CharClass    string `json:"class,omitempty"`
	Gender       string `json:"gender,omitempty"`
	Message      string `json:"message,omitempty"`
}

// MoveEventResponse — action_id: 2001 (broadcast khi ai đó di chuyển)
type MoveEventResponse struct {
	ActionID    int    `json:"action_id"`
	CharacterID int    `json:"character_id"`
	MapID       int    `json:"map_id"`
	Name        string `json:"name"`
	CurrentX    int    `json:"current_x"`
	CurrentY    int    `json:"current_y"`
	Direction   string `json:"direction"`
	MountType   string `json:"mount_type"`
	Timestamp   int64  `json:"timestamp"`
}

// CombatResultResponse — action_id: 2002
type CombatResultResponse struct {
	ActionID   int  `json:"action_id"`
	AttackerID int  `json:"attacker_id"`
	TargetID   int  `json:"target_id"`
	Damage     int  `json:"damage"`
	IsCrit     bool `json:"is_crit"`
	IsDead     bool `json:"is_dead"`
}

// CreateCharacterResponse — action_id: 2003
type CreateCharacterResponse struct {
	ActionID    int    `json:"action_id"`
	Status      string `json:"status"`
	CharacterID int    `json:"character_id,omitempty"`
	Name        string `json:"name,omitempty"`
	Level       int    `json:"level,omitempty"`
	HP          int    `json:"hp,omitempty"`
	HPMax       int    `json:"hp_max,omitempty"`
	Faction     string `json:"faction,omitempty"`
	CharClass   string `json:"class,omitempty"`
	Gender      string `json:"gender,omitempty"`
	Message     string `json:"message,omitempty"`
}

// ChatBroadcastResponse — action_id: 2005
type ChatBroadcastResponse struct {
	ActionID    int    `json:"action_id"`
	SenderID    int    `json:"sender_id"`
	SenderName  string `json:"sender_name"`
	ChatChannel string `json:"chat_channel"`
	Message     string `json:"message"`
	Timestamp   int64  `json:"timestamp"`
}

// QuestSyncResponse — action_id: 2008
type QuestSyncResponse struct {
	ActionID  int    `json:"action_id"`
	QuestID   int    `json:"quest_id"`
	Status    string `json:"status"`
	Name      string `json:"name"`
	Level     int    `json:"level"`
	HP        int    `json:"hp"`
	HPMax     int    `json:"hp_max"`
	Gold      int    `json:"gold"`
	Message   string `json:"message"`
}

// ErrorResponse — generic error response
type ErrorResponse struct {
	ActionID int    `json:"action_id"`
	Status   string `json:"status"`
	Message  string `json:"message"`
}
