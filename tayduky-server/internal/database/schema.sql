-- 1. Bảng tài khoản người dùng
CREATE TABLE accounts (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(100),
    status INT DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Bảng nhân vật trong game
CREATE TABLE characters (
    id SERIAL PRIMARY KEY,
    account_id INT REFERENCES accounts(id) ON DELETE CASCADE,
    name VARCHAR(50) UNIQUE NOT NULL,
    faction VARCHAR(20) NOT NULL CHECK (faction IN ('Thần Tộc', 'Ma Tộc', 'Yêu Tộc', 'Chưa Vào')),
    level INT DEFAULT 1,
    exp INT DEFAULT 0,
    vip_level INT DEFAULT 0,
    hp_max INT DEFAULT 100,
    hp_current INT DEFAULT 100,
    mp_max INT DEFAULT 50,
    mp_current INT DEFAULT 50,
    gold INT DEFAULT 0,
    knb INT DEFAULT 0,
    map_id INT DEFAULT 1,
    pos_x INT DEFAULT 12,
    pos_y INT DEFAULT 8,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Bảng danh mục Thú cưỡi (Tọa kỵ)
CREATE TABLE character_mounts (
    id SERIAL PRIMARY KEY,
    character_id INT REFERENCES characters(id) ON DELETE CASCADE,
    mount_type VARCHAR(50) NOT NULL,
    level INT DEFAULT 1,
    speed_bonus INT NOT NULL,
    is_equipped BOOLEAN DEFAULT FALSE
);

-- 4. Bảng danh sách Tiên sủng (Pets) của nhân vật
CREATE TABLE character_pets (
    id SERIAL PRIMARY KEY,
    character_id INT REFERENCES characters(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL,
    pet_type VARCHAR(50) NOT NULL,
    grade VARCHAR(20) NOT NULL CHECK (grade IN ('Trân Thú', 'Tán Tiên', 'Kim Tiên')),
    level INT DEFAULT 1,
    exp INT DEFAULT 0,
    str INT DEFAULT 10,
    int_stat INT DEFAULT 10,
    vit INT DEFAULT 10,
    agi INT DEFAULT 10,
    hp_max INT DEFAULT 80,
    hp_current INT DEFAULT 80,
    skills JSONB,
    is_summoned BOOLEAN DEFAULT FALSE
);

-- 5. Bảng hành trang của nhân vật (Inventory)
CREATE TABLE inventories (
    id SERIAL PRIMARY KEY,
    character_id INT REFERENCES characters(id) ON DELETE CASCADE,
    slot_index INT NOT NULL,
    item_id INT NOT NULL,
    quantity INT DEFAULT 1,
    is_bound BOOLEAN DEFAULT FALSE,
    UNIQUE(character_id, slot_index)
);

-- 6. Bảng tiến trình nhiệm vụ của nhân vật
CREATE TABLE character_quests (
    id SERIAL PRIMARY KEY,
    character_id INT REFERENCES characters(id) ON DELETE CASCADE,
    quest_id INT NOT NULL,
    status VARCHAR(20) DEFAULT 'IN_PROGRESS' CHECK (status IN ('IN_PROGRESS', 'COMPLETED')),
    progress_count INT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
