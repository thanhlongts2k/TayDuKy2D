package main

import (
	"fmt"
	"log"
	"net"
	"net/http"
)

func main() {
	fmt.Println("Starting Tay Du Ky Mobile Server...")
	
	// Start TCP listener for mobile client
	go startTCPServer()

	// Start WebSocket listener for web client
	startWebSocketServer()
}

func startTCPServer() {
	listener, err := net.Listen("tcp", ":8080")
	if err != nil {
		log.Fatalf("Failed to listen on TCP: %v", err)
	}
	defer listener.Close()
	fmt.Println("TCP Server listening on port 8080")
	
	for {
		conn, err := listener.Accept()
		if err != nil {
			log.Printf("Failed to accept TCP connection: %v", err)
			continue
		}
		go handleTCPConnection(conn)
	}
}

func handleTCPConnection(conn net.Conn) {
	defer conn.Close()
	fmt.Printf("New TCP Client connected: %s\n", conn.RemoteAddr().String())
	// Handle packets here...
}

func startWebSocketServer() {
	http.HandleFunc("/ws", func(w http.ResponseWriter, r *http.Request) {
		fmt.Println("New WebSocket client connected")
		// Upgrade connection and handle here...
	})

	fmt.Println("WebSocket Server listening on port 8081")
	log.Fatal(http.ListenAndServe(":8081", nil))
}
