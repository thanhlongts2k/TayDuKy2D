package models

import "time"

// Character represents a player character in the game.
type Character struct {
	ID         int       `gorm:"primaryKey" json:"id"`
	AccountID  int       `gorm:"index" json:"account_id"`
	Name       string    `gorm:"uniqueIndex;size:50" json:"name"`
	Faction    string    `gorm:"size:20" json:"faction"` // 'Thần Tộc', 'Ma Tộc', 'Yêu Tộc', 'Chưa Vào'
	Level      int       `gorm:"default:1" json:"level"`
	Exp        int       `gorm:"default:0" json:"exp"`
	VIPLevel   int       `gorm:"default:0" json:"vip_level"`
	HPMax      int       `gorm:"default:100" json:"hp_max"`
	HPCurrent  int       `gorm:"default:100" json:"hp_current"`
	MPMax      int       `gorm:"default:50" json:"mp_max"`
	MPCurrent  int       `gorm:"default:50" json:"mp_current"`
	Gold       int       `gorm:"default:0" json:"gold"`
	KNB        int       `gorm:"default:0" json:"knb"`
	MapID      int       `gorm:"default:1" json:"map_id"`
	PosX       int       `gorm:"default:12" json:"pos_x"`
	PosY       int       `gorm:"default:8" json:"pos_y"`
	CreatedAt  time.Time `json:"created_at"`
}

// CharacterMount represents character mounts.
type CharacterMount struct {
	ID          int    `gorm:"primaryKey" json:"id"`
	CharacterID int    `gorm:"index" json:"character_id"`
	MountType   string `gorm:"size:50" json:"mount_type"` // 'Bạch Hổ', 'Hỏa Kỳ Lân', 'Phi Kiếm'
	Level       int    `gorm:"default:1" json:"level"`
	SpeedBonus  int    `json:"speed_bonus"`
	IsEquipped  bool   `gorm:"default:false" json:"is_equipped"`
}

// CharacterPet represents character tiên sủng pets.
type CharacterPet struct {
	ID          int    `gorm:"primaryKey" json:"id"`
	CharacterID int    `gorm:"index" json:"character_id"`
	Name        string `gorm:"size:50" json:"name"`
	PetType     string `gorm:"size:50" json:"pet_type"`
	Grade       string `gorm:"size:20" json:"grade"` // 'Trân Thú', 'Tán Tiên', 'Kim Tiên'
	Level       int    `gorm:"default:1" json:"level"`
	Exp         int    `gorm:"default:0" json:"exp"`
	Str         int    `gorm:"default:10" json:"str"`
	IntStat     int    `gorm:"default:10" json:"int_stat"`
	Vit         int    `gorm:"default:10" json:"vit"`
	Agi         int    `gorm:"default:10" json:"agi"`
	HPMax       int    `gorm:"default:80" json:"hp_max"`
	HPCurrent   int    `gorm:"default:80" json:"hp_current"`
	Skills      string `gorm:"type:jsonb" json:"skills"` // JSON array of pet skills
	IsSummoned  bool   `gorm:"default:false" json:"is_summoned"`
}

// MovePacket represents incoming action_id 1001 payload.
type MovePacket struct {
	ActionID    int    `json:"action_id"`
	CharacterID int    `json:"character_id"`
	TargetX     int    `json:"target_x"`
	TargetY     int    `json:"target_y"`
	Direction   string `json:"direction"`
	IsRiding    bool   `json:"is_riding"`
}
