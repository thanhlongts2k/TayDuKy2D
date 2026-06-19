package move

import (
	"fmt"
	"math"

	"tayduky-server/internal/database"
	"tayduky-server/internal/models"
)

// GridSize represents dimensions of coordinate tiles
const GridSize = 1.0
const MaxMoveSpeed = 10.0 // Max tile distance allowed per tick

// HandleMoveRequest validates move request and updates Redis cache.
func HandleMoveRequest(packet models.MovePacket) (bool, string) {
	// 1. Retrieve current player coordinates from Redis Cache
	posStr, err := database.GetPosition(packet.CharacterID)
	
	// If no coordinate cache exists (e.g. first login), set defaults and allow move
	if err != nil {
		err = database.SetPosition(packet.CharacterID, 1, packet.TargetX, packet.TargetY)
		if err != nil {
			return false, fmt.Sprintf("Failed to initialize coordinates in Cache: %v", err)
		}
		return true, "Successfully initialized position"
	}

	var currentMap, currentX, currentY int
	_, err = fmt.Sscanf(posStr, "%d,%d,%d", &currentMap, &currentX, &currentY)
	if err != nil {
		return false, fmt.Sprintf("Corrupt coordinates data in Cache: %s", posStr)
	}

	// 2. Perform distance verification (Check speed hacks)
	deltaX := float64(packet.TargetX - currentX)
	deltaY := float64(packet.TargetY - currentY)
	distance := math.Sqrt(deltaX*deltaX + deltaY*deltaY)

	if distance > MaxMoveSpeed {
		return false, fmt.Sprintf("Move blocked: Speed limit exceeded. Distance=%.2f", distance)
	}

	// 3. Map collision validation (Place obstacles coordinate array here)
	// (E.g. block coordinates 0,0 for map boundaries)
	if packet.TargetX < 0 || packet.TargetY < 0 {
		return false, "Move blocked: Out of map boundaries"
	}

	// 4. Update coordinates in Redis Cache
	err = database.SetPosition(packet.CharacterID, currentMap, packet.TargetX, packet.TargetY)
	if err != nil {
		return false, fmt.Sprintf("Failed to cache position: %v", err)
	}

	return true, "Move validated and updated"
}
