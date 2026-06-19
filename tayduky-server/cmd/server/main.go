package main

import (
	"fmt"
	"log"
	"net"
	"net/http"
	"os"

	"tayduky-server/internal/database"
	"tayduky-server/internal/gateway"
)

func main() {
	fmt.Println("===================================================")
	fmt.Println("  Tay Du Ky Mobile — Golang Game Server v0.2")
	fmt.Println("===================================================")

	// 1. Kết nối Redis (bắt buộc để quản lý vị trí player thời gian thực)
	redisAddr := getEnv("REDIS_ADDR", "localhost:6379")
	database.InitRedis(redisAddr, "", 0)

	// 2. Kết nối PostgreSQL (tùy chọn — bỏ qua nếu chưa cấu hình)
	pgHost := getEnv("PG_HOST", "")
	if pgHost != "" {
		database.InitPostgres(pgHost, 5432,
			getEnv("PG_USER", "postgres"),
			getEnv("PG_PASSWORD", ""),
			getEnv("PG_DBNAME", "tayduky"),
		)
		defer database.ClosePostgres()
	} else {
		fmt.Println("[INFO] PostgreSQL not configured (PG_HOST not set). Running in In-Memory mode.")
	}

	// 3. Khởi tạo ClientManager — quản lý connections online
	manager := gateway.NewClientManager()

	// 4. Chạy WebSocket Server (port 8081) trong goroutine riêng
	go startWebSocketServer(manager)

	// 5. Chạy TCP Server (port 8080) — blocking main goroutine
	startTCPServer(manager)
}

// startTCPServer lắng nghe kết nối TCP trên port 8080.
// Mỗi kết nối mới được xử lý trong goroutine riêng biệt.
func startTCPServer(manager *gateway.ClientManager) {
	listener, err := net.Listen("tcp", ":8080")
	if err != nil {
		log.Fatalf("[FATAL] Failed to start TCP Server: %v", err)
	}
	defer listener.Close()

	fmt.Println("[TCP] Server listening on port 8080")

	for {
		conn, err := listener.Accept()
		if err != nil {
			log.Printf("[ERROR] Failed to accept connection: %v", err)
			continue
		}
		fmt.Printf("[TCP] New client connected: %s\n", conn.RemoteAddr())
		go gateway.HandleTCPClient(conn, manager)
	}
}

// startWebSocketServer lắng nghe kết nối WebSocket trên port 8081 (dành cho Web Client).
// TODO Sprint 2: Implement đầy đủ WebSocket upgrade và dispatch giống TCP
func startWebSocketServer(manager *gateway.ClientManager) {
	http.HandleFunc("/ws", func(w http.ResponseWriter, r *http.Request) {
		// TODO: Upgrade HTTP -> WebSocket, sau đó gọi gateway.HandleWebSocketClient()
		fmt.Printf("[WS] New WebSocket connection attempt from %s\n", r.RemoteAddr)
		w.WriteHeader(http.StatusNotImplemented)
	})

	fmt.Println("[WS] WebSocket Server listening on port 8081 (stub — TCP is primary)")
	if err := http.ListenAndServe(":8081", nil); err != nil {
		log.Fatalf("[FATAL] Failed to start WebSocket Server: %v", err)
	}
}

// getEnv đọc biến môi trường với giá trị mặc định fallback.
func getEnv(key, defaultVal string) string {
	if val := os.Getenv(key); val != "" {
		return val
	}
	return defaultVal
}
