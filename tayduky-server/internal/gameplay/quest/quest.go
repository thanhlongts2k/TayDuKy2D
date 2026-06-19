package quest

import (
	"encoding/json"
	"fmt"
	"os"
)

// QuestConfig ánh xạ cấu hình nhiệm vụ từ quests.json
type QuestConfig struct {
	ID          int    `json:"id"`
	Name        string `json:"name"`
	Description string `json:"description"`
	Type        string `json:"type"`      // "kill", "collect", "talk"
	TargetID    int    `json:"target_id"` // ID quái / item / NPC
	TargetCount int    `json:"target_count"`
	RewardExp   int    `json:"reward_exp"`
	RewardGold  int    `json:"reward_gold"`
	NextQuestID int    `json:"next_quest_id"` // Nhiệm vụ tiếp theo (chuỗi)
}

// QuestDB lưu toàn bộ nhiệm vụ đã load từ file.
var QuestDB = map[int]*QuestConfig{}

// LoadQuestsFromFile nạp dữ liệu nhiệm vụ từ file JSON.
func LoadQuestsFromFile(path string) error {
	data, err := os.ReadFile(path)
	if err != nil {
		return fmt.Errorf("failed to read quests file: %w", err)
	}

	var quests []QuestConfig
	if err := json.Unmarshal(data, &quests); err != nil {
		return fmt.Errorf("failed to parse quests JSON: %w", err)
	}

	for i := range quests {
		QuestDB[quests[i].ID] = &quests[i]
	}

	fmt.Printf("[QUEST] Loaded %d quests from %s\n", len(QuestDB), path)
	return nil
}

// GetQuest trả về cấu hình nhiệm vụ theo ID.
func GetQuest(questID int) (*QuestConfig, bool) {
	q, ok := QuestDB[questID]
	return q, ok
}

// CalculateReward tính phần thưởng EXP và Gold cho một nhiệm vụ.
// Trả về (expReward, goldReward).
func CalculateReward(questID int) (int, int) {
	if q, ok := QuestDB[questID]; ok {
		return q.RewardExp, q.RewardGold
	}
	// Giá trị mặc định nếu quest không tồn tại trong DB (phòng tránh crash)
	return 200, 100
}
