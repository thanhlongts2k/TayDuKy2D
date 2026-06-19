package gateway

import (
	"fmt"
	"net"
	"sync"
)

// Client đại diện cho một kết nối đang active của người chơi.
type Client struct {
	Conn        net.Conn
	CharacterID int
	Username    string
	MapID       int
	PosX        int
	PosY        int
}

// ClientManager quản lý thread-safe tất cả kết nối đang online.
// Dùng thay cho slice `clients` trong server.py.
type ClientManager struct {
	clients map[net.Conn]*Client // Conn -> Client
	mu      sync.RWMutex
}

// NewClientManager khởi tạo một ClientManager mới.
func NewClientManager() *ClientManager {
	return &ClientManager{
		clients: make(map[net.Conn]*Client),
	}
}

// Register thêm một kết nối mới vào danh sách.
func (cm *ClientManager) Register(conn net.Conn) *Client {
	client := &Client{Conn: conn, MapID: 101, PosX: 12, PosY: 8}
	cm.mu.Lock()
	cm.clients[conn] = client
	cm.mu.Unlock()
	fmt.Printf("[GATEWAY] Client registered: %s (Total online: %d)\n", conn.RemoteAddr(), cm.Count())
	return client
}

// Unregister xóa kết nối khỏi danh sách khi ngắt kết nối.
func (cm *ClientManager) Unregister(conn net.Conn) {
	cm.mu.Lock()
	if client, ok := cm.clients[conn]; ok {
		fmt.Printf("[GATEWAY] Client unregistered: %s (CharID: %d)\n", conn.RemoteAddr(), client.CharacterID)
		delete(cm.clients, conn)
	}
	cm.mu.Unlock()
}

// GetClient lấy thông tin Client từ connection.
func (cm *ClientManager) GetClient(conn net.Conn) (*Client, bool) {
	cm.mu.RLock()
	defer cm.mu.RUnlock()
	client, ok := cm.clients[conn]
	return client, ok
}

// Broadcast gửi một message đến tất cả clients trên cùng mapID,
// ngoại trừ sender (giống hàm broadcast() trong server.py).
func (cm *ClientManager) Broadcast(data []byte, senderConn net.Conn, mapID int) {
	cm.mu.RLock()
	defer cm.mu.RUnlock()

	for conn, client := range cm.clients {
		if conn == senderConn {
			continue
		}
		// Chỉ gửi cho người chơi trên cùng bản đồ (AOI cơ bản)
		if mapID == 0 || client.MapID == mapID {
			if _, err := conn.Write(data); err != nil {
				fmt.Printf("[GATEWAY] Broadcast error to %s: %v\n", conn.RemoteAddr(), err)
			}
		}
	}
}

// BroadcastAll gửi đến tất cả clients (ví dụ: tin nhắn chat thế giới).
func (cm *ClientManager) BroadcastAll(data []byte) {
	cm.mu.RLock()
	defer cm.mu.RUnlock()

	for conn := range cm.clients {
		if _, err := conn.Write(data); err != nil {
			fmt.Printf("[GATEWAY] BroadcastAll error to %s: %v\n", conn.RemoteAddr(), err)
		}
	}
}

// Send gửi message trực tiếp đến một connection cụ thể.
func (cm *ClientManager) Send(conn net.Conn, data []byte) error {
	_, err := conn.Write(data)
	return err
}

// Count trả về số lượng client đang online.
func (cm *ClientManager) Count() int {
	cm.mu.RLock()
	defer cm.mu.RUnlock()
	return len(cm.clients)
}
