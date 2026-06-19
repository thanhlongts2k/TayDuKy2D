package database

import (
	"database/sql"
	"fmt"
	"log"

	_ "github.com/lib/pq"
)

var DB *sql.DB

// InitPostgres connects to the PostgreSQL database.
func InitPostgres(host string, port int, user string, password string, dbname string) {
	connStr := fmt.Sprintf("host=%s port=%d user=%s password=%s dbname=%s sslmode=disable",
		host, port, user, password, dbname)
	
	var err error
	DB, err = sql.Open("postgres", connStr)
	if err != nil {
		log.Fatalf("Error opening connection to PostgreSQL: %v", err)
	}

	err = DB.Ping()
	if err != nil {
		log.Fatalf("Error pinging PostgreSQL: %v", err)
	}

	fmt.Println("Successfully connected to PostgreSQL Database!")
}

// ClosePostgres closes the database connection.
func ClosePostgres() {
	if DB != nil {
		DB.Close()
		fmt.Println("PostgreSQL connection closed.")
	}
}
