package chat

import (
	"encoding/json"
	"fmt"
	"time"

	"tayduky-server/internal/database"
)

// MessagePayload represents a structured chat message packet.
type MessagePayload struct {
	SenderID    int    `json:"sender_id"`
	SenderName  string `json:"sender_name"`
	Channel     string `json:"channel"` // "WORLD", "GUILD", "WHISPER"
	Content     string `json:"content"`
	Timestamp   int64  `json:"timestamp"`
}

// BroadcastMessage publishes a chat message to Redis Pub/Sub for server-wide sync.
func BroadcastMessage(senderID int, name string, channel string, content string) error {
	msg := MessagePayload{
		SenderID:   senderID,
		SenderName: name,
		Channel:    channel,
		Content:    content,
		Timestamp:  time.Now().Unix(),
	}

	payloadBytes, err := json.Marshal(msg)
	if err != nil {
		return fmt.Errorf("failed to marshal chat packet: %w", err)
	}

	redisChannel := "pubsub:chat:" + channel
	err = database.RDB.Publish(database.Ctx, redisChannel, payloadBytes).Err()
	if err != nil {
		return fmt.Errorf("failed to publish message to Redis: %w", err)
	}

	// For WORLD channel, save to world chat history list (keep last 50 messages)
	if channel == "WORLD" {
		listKey := "chat:world"
		database.RDB.LPush(database.Ctx, listKey, payloadBytes)
		database.RDB.LTrim(database.Ctx, listKey, 0, 49) // Maintain max 50 items
	}

	return nil
}

// SubscribeToChannel listens to a Redis channel and prints messages (runs asynchronously).
func SubscribeToChannel(channel string, messageHandler func(payload MessagePayload)) {
	redisChannel := "pubsub:chat:" + channel
	pubsub := database.RDB.Subscribe(database.Ctx, redisChannel)

	go func() {
		defer pubsub.Close()
		ch := pubsub.Channel()
		for msg := range ch {
			var payload MessagePayload
			err := json.Unmarshal([]byte(msg.Payload), &payload)
			if err == nil {
				messageHandler(payload)
			}
		}
	}()
}
