package database

import (
	"context"
	"fmt"
	"log"
	"time"

	"github.com/go-redis/redis/v8"
)

var RDB *redis.Client
var Ctx = context.Background()

// InitRedis connects to the Redis server.
func InitRedis(addr string, password string, db int) {
	RDB = redis.NewClient(&redis.Options{
		Addr:     addr,
		Password: password,
		DB:       db,
	})

	_, err := RDB.Ping(Ctx).Result()
	if err != nil {
		log.Fatalf("Error connecting to Redis: %v", err)
	}

	fmt.Println("Successfully connected to Redis Server!")
}

// SetPosition stores player grid coordinates in Redis Hash.
func SetPosition(characterID int, mapID int, x int, y int) error {
	key := "char:position"
	field := fmt.Sprintf("%d", characterID)
	value := fmt.Sprintf("%d,%d,%d", mapID, x, y)
	
	return RDB.HSet(Ctx, key, field, value).Err()
}

// GetPosition retrieves player coordinates from Redis Hash.
func GetPosition(characterID int) (string, error) {
	key := "char:position"
	field := fmt.Sprintf("%d", characterID)
	
	return RDB.HGet(Ctx, key, field).Result()
}

// SetSession sets user session token with 1 hour expiration.
func SetSession(characterID int, token string) error {
	key := fmt.Sprintf("session:%d", characterID)
	return RDB.Set(Ctx, key, token, time.Hour).Err()
}
