package combat

import (
	"math/rand"
	"time"
)

func init() {
	rand.Seed(time.Now().UnixNano())
}

// Entity represents an actor in combat (Player or Monster).
type Entity struct {
	ID        int    `json:"id"`
	Name      string `json:"name"`
	Level     int    `json:"level"`
	Faction   string `json:"faction"` // 'Thần Tộc', 'Ma Tộc', 'Yêu Tộc'
	Attack    int    `json:"attack"`
	Defense   int    `json:"defense"`
	HPMax     int    `json:"hp_max"`
	HPCurrent int    `json:"hp_current"`
	CritRate  float64 `json:"crit_rate"` // e.g., 0.15 for 15%
}

// CombatResult represents the outcome of a single combat round.
type CombatResult struct {
	AttackerID int  `json:"attacker_id"`
	TargetID   int  `json:"target_id"`
	Damage     int  `json:"damage"`
	IsCrit     bool `json:"is_crit"`
	IsDead     bool `json:"is_dead"`
}

// ExecuteAttack calculates damage from attacker to target, applying criticals and faction multipliers.
func ExecuteAttack(attacker *Entity, target *Entity) CombatResult {
	result := CombatResult{
		AttackerID: attacker.ID,
		TargetID:   target.ID,
	}

	// 1. Calculate Base Damage: Attack - Defense (minimum 1 damage)
	baseDamage := attacker.Attack - target.Defense
	if baseDamage <= 0 {
		baseDamage = 1
	}

	// 2. Apply Faction Modifiers (Thần khắc Yêu, Yêu khắc Ma, Ma khắc Thần)
	multiplier := 1.0
	switch attacker.Faction {
	case "Thần Tộc":
		if target.Faction == "Yêu Tộc" {
			multiplier = 1.25 // Thần Tộc counters Yêu Tộc (25% extra dmg)
		}
	case "Yêu Tộc":
		if target.Faction == "Ma Tộc" {
			multiplier = 1.25 // Yêu Tộc counters Ma Tộc
		}
	case "Ma Tộc":
		if target.Faction == "Thần Tộc" {
			multiplier = 1.25 // Ma Tộc counters Thần Tộc
		}
	}
	
	finalDamage := float64(baseDamage) * multiplier

	// 3. Roll for Critical Hit
	roll := rand.Float64()
	if roll <= attacker.CritRate {
		finalDamage *= 1.5 // 150% damage on Critical hits
		result.IsCrit = true
	}

	result.Damage = int(mathRound(finalDamage))

	// 4. Apply damage to target
	target.HPCurrent -= result.Damage
	if target.HPCurrent <= 0 {
		target.HPCurrent = 0
		result.IsDead = true
	}

	return result
}

func mathRound(val float64) int {
	return int(val + 0.5)
}
