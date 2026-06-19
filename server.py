import asyncio
import json
import math
import time
import random
import os

# Load quests.json dynamically from client Resources
QUESTS_CONFIG_PATH = os.path.join(os.path.dirname(__file__), "tayduky-client", "Assets", "Resources", "Quests", "quests.json")
quests_db = {}
try:
    if os.path.exists(QUESTS_CONFIG_PATH):
        with open(QUESTS_CONFIG_PATH, "r", encoding="utf-8") as f:
            quests_list = json.load(f)
            for q in quests_list:
                quests_db[q["id"]] = q
        print(f"[START] Successfully loaded {len(quests_db)} quests from configuration.")
    else:
        print(f"[WARNING] Quest configuration file not found at: {QUESTS_CONFIG_PATH}")
except Exception as e:
    print(f"[ERROR] Failed to load quests.json: {e}")

# In-memory quest logs tracking progress
character_quests = {} # Key: char_id, Value: dict of {quest_id: {"status": status, "progress": count}}

HOST = '0.0.0.0'
PORT = 8080
MAX_MOVE_SPEED = 10.0 # Maximum grid tile distance allowed per move packet

# List of active client writers
clients = []

# In-memory database
accounts = {
    "admin": "admin",
    "shinichi": "123"
}
account_characters = {
    "admin": 1024,
    "shinichi": 1024
}

player_positions = {
    1024: (101, 12, 5)
}
player_stats = {
    1024: {
        "name": "shinichi",
        "level": 32,
        "hp": 100,
        "hp_max": 100,
        "faction": "Thần Tộc",
        "class": "Pháp Sư",
        "gender": "Nam",
        "crit_rate": 0.15,
        "attack": 25,
        "defense": 10,
        "exp": 0,
        "gold": 0
    }
}
monster_stats = {
    999: {"name": "Tiểu Toàn Phong", "level": 15, "hp": 60, "hp_max": 60, "faction": "Yêu Tộc", "crit_rate": 0.05, "attack": 12, "defense": 5}
}

async def broadcast(payload_str, exclude_writer=None):
    """Broadcasts a JSON string to all connected asyncio TCP clients."""
    data = payload_str.encode('utf-8')
    disconnected_clients = []
    for writer in clients:
        if writer != exclude_writer:
            try:
                writer.write(data)
                await writer.drain()
            except Exception:
                disconnected_clients.append(writer)
    
    for writer in disconnected_clients:
        if writer in clients:
            clients.remove(writer)

async def handle_client(reader, writer):
    addr = writer.get_extra_info('peername')
    print(f"[CONNECTED] New client joined from {addr}")
    clients.append(writer)

    try:
        while True:
            data = await reader.read(4096)
            if not data:
                break
            
            raw_payload = data.decode('utf-8').strip()
            print(f"[RECEIVED] From {addr}: {raw_payload}")
            
            try:
                packet = json.loads(raw_payload)
                action_id = packet.get("action_id")
                
                if action_id == 1000:
                    await handle_login(packet, writer)
                elif action_id == 1004:
                    await handle_register(packet, writer)
                elif action_id == 1003:
                    await handle_create_character(packet, writer)
                elif action_id == 1001:
                    await handle_move(packet, writer)
                elif action_id == 1002:
                    await handle_combat(packet, writer)
                elif action_id == 1005:
                    await handle_chat(packet, writer)
                elif action_id == 1008:
                    await handle_quest(packet, writer)
                elif action_id == 1010:
                    await handle_mount(packet, writer)
                elif action_id == 1011:
                    await handle_pet(packet, writer)
                else:
                    print(f"[WARNING] Unknown action_id: {action_id}")
            except json.JSONDecodeError:
                print(f"[ERROR] Invalid JSON formatting: {raw_payload}")
    except Exception as e:
        print(f"[ERROR] Client connection exception: {e}")
    finally:
        if writer in clients:
            clients.remove(writer)
        writer.close()
        await writer.wait_closed()
        print(f"[DISCONNECTED] Client {addr} left")

async def send_response(writer, payload):
    try:
        data = json.dumps(payload).encode('utf-8')
        writer.write(data)
        await writer.drain()
    except Exception as e:
        print(f"[ERROR] Failed to send response: {e}")

async def handle_login(packet, writer):
    username = packet.get("username")
    password = packet.get("password")
    
    print(f"[LOGIN REQUEST] User={username}")
    
    # Check credentials
    if username in accounts and accounts[username] == password:
        # Check if user has character
        if username in account_characters:
            char_id = account_characters[username]
            stats = player_stats.get(char_id, {})
            
            response = {
                "action_id": 2000,
                "status": "success",
                "has_character": True,
                "character_id": char_id,
                "name": stats.get("name", "Unknown"),
                "level": stats.get("level", 1),
                "hp": stats.get("hp", 100),
                "hp_max": stats.get("hp_max", 100),
                "faction": stats.get("faction", "Thần Tộc"),
                "class": stats.get("class", "Kiếm Tiên"),
                "gender": stats.get("gender", "Nam")
            }
        else:
            response = {
                "action_id": 2000,
                "status": "success",
                "has_character": False
            }
    else:
        response = {
            "action_id": 2000,
            "status": "fail",
            "message": "Sai tài khoản hoặc mật khẩu!"
        }
        
    await send_response(writer, response)

async def handle_register(packet, writer):
    username = packet.get("username")
    password = packet.get("password")
    
    print(f"[REGISTER REQUEST] User={username}")
    
    if username in accounts:
        response = {
            "action_id": 2000, # Login response with fail status
            "status": "fail",
            "message": "Tài khoản này đã tồn tại!"
        }
    else:
        # Register new account
        accounts[username] = password
        response = {
            "action_id": 2000,
            "status": "success",
            "has_character": False
        }
        print(f"[REGISTER SUCCESS] Registered new account: {username}")
        
    await send_response(writer, response)

async def handle_create_character(packet, writer):
    username = packet.get("username")
    name = packet.get("name")
    faction = packet.get("faction", "Thần Tộc")
    char_class = packet.get("class", "Kiếm Tiên")
    gender = packet.get("gender", "Nam")
    
    print(f"[CREATE CHAR REQUEST] User={username}, Name={name}, Faction={faction}, Class={char_class}, Gender={gender}")
    
    # Check name duplicate
    name_exists = any(stats["name"].lower() == name.lower() for stats in player_stats.values())
    
    if name_exists:
        response = {
            "action_id": 2003,
            "status": "fail",
            "message": "Tên nhân vật đã tồn tại!"
        }
    else:
        # Create character
        char_id = 1000 + len(player_stats) + 1
        
        # Fine-grained base stats according to selected playable class
        stats = {
            "name": name,
            "level": 1,
            "faction": faction,
            "class": char_class,
            "gender": gender
        }

        if char_class == "Kiếm Tiên":
            stats.update({"hp": 120, "hp_max": 120, "crit_rate": 0.12, "attack": 22, "defense": 12, "exp": 0, "gold": 0})
        elif char_class == "Pháp Sư":
            stats.update({"hp": 90, "hp_max": 90, "crit_rate": 0.15, "attack": 28, "defense": 8, "exp": 0, "gold": 0})
        elif char_class == "Ma Đao":
            stats.update({"hp": 130, "hp_max": 130, "crit_rate": 0.20, "attack": 22, "defense": 12, "exp": 0, "gold": 0})
        elif char_class == "Sát Thủ":
            stats.update({"hp": 110, "hp_max": 110, "crit_rate": 0.25, "attack": 24, "defense": 9, "exp": 0, "gold": 0})
        elif char_class == "Yêu Vương":
            stats.update({"hp": 150, "hp_max": 150, "crit_rate": 0.08, "attack": 19, "defense": 16, "exp": 0, "gold": 0})
        elif char_class == "Yêu Pháp":
            stats.update({"hp": 120, "hp_max": 120, "crit_rate": 0.12, "attack": 21, "defense": 11, "exp": 0, "gold": 0})
        else: # Default
            stats.update({"hp": 100, "hp_max": 100, "crit_rate": 0.10, "attack": 20, "defense": 10, "exp": 0, "gold": 0})
            
        player_stats[char_id] = stats
        account_characters[username] = char_id
        player_positions[char_id] = (101, 12, 5) # Default to map 101, x=12, y=5
        
        response = {
            "action_id": 2003,
            "status": "success",
            "character_id": char_id,
            "name": name,
            "level": 1,
            "hp": stats["hp"],
            "hp_max": stats["hp_max"],
            "faction": faction,
            "class": char_class,
            "gender": gender
        }
        print(f"[CREATE CHAR SUCCESS] Character created for {username}: {name} (ID: {char_id}, Class: {char_class})")
        
    await send_response(writer, response)

async def handle_move(packet, writer):
    char_id = packet.get("character_id")
    map_id = packet.get("map_id", 101)
    target_x = packet.get("target_x")
    target_y = packet.get("target_y")
    direction = packet.get("direction", "EAST")
    is_riding = packet.get("is_riding", False)

    # Calculate distance and validate speed hacks (only if on same map)
    if char_id in player_positions:
        pos_info = player_positions[char_id]
        if len(pos_info) == 2:
            cur_map, cur_x, cur_y = 101, pos_info[0], pos_info[1]
        else:
            cur_map, cur_x, cur_y = pos_info[0], pos_info[1], pos_info[2]

        if cur_map == map_id:
            dist = math.sqrt((target_x - cur_x)**2 + (target_y - cur_y)**2)
            if dist > MAX_MOVE_SPEED:
                print(f"[MOVE BLOCKED] Speed limit exceeded for player {char_id}. Distance={dist:.2f}")
                return
    
    # Update position in-memory (map_id, x, y)
    player_positions[char_id] = (map_id, target_x, target_y)
    print(f"[MOVE] Character {char_id} moved on Map {map_id} to ({target_x}, {target_y})")

    # Broadcast position update to all other players (AOI Action ID 2001)
    response = {
        "action_id": 2001,
        "character_id": char_id,
        "map_id": map_id,
        "name": player_stats.get(char_id, {}).get("name", "Unknown"),
        "current_x": target_x,
        "current_y": target_y,
        "direction": direction,
        "mount_type": "Hỏa Kỳ Lân" if is_riding else "Chân chạy",
        "timestamp": int(time.time())
    }
    await broadcast(json.dumps(response), exclude_writer=writer)

async def handle_combat(packet, writer):
    attacker_id = packet.get("attacker_id")
    target_id = packet.get("target_id")

    attacker = player_stats.get(attacker_id)
    target = monster_stats.get(target_id)

    if not attacker or not target:
        print(f"[COMBAT ERROR] Attacker/Target not found. Attacker={attacker_id}, Target={target_id}")
        return

    # Base damage calculation
    base_dmg = attacker["attack"] - target["defense"]
    if base_dmg <= 0:
        base_dmg = 1

    # Faction multiplier calculation (Thần khắc Yêu, Yêu khắc Ma, Ma khắc Thần)
    multiplier = 1.0
    if attacker["faction"] == "Thần Tộc" and target["faction"] == "Yêu Tộc":
        multiplier = 1.25
    elif attacker["faction"] == "Yêu Tộc" and target["faction"] == "Ma Tộc":
        multiplier = 1.25
    elif attacker["faction"] == "Ma Tộc" and target["faction"] == "Thần Tộc":
        multiplier = 1.25

    final_dmg = base_dmg * multiplier

    # Crit rate check
    is_crit = random.random() <= attacker["crit_rate"]
    if is_crit:
        final_dmg *= 1.5

    final_dmg = int(final_dmg + 0.5)
    target["hp"] -= final_dmg
    is_dead = False
    if target["hp"] <= 0:
        target["hp"] = 0
        is_dead = True
        # Reset monster HP for next fight
        target["hp"] = target["hp_max"]

    print(f"[COMBAT] {attacker['name']} hit {target['name']} for {final_dmg} damage (Crit={is_crit}, Dead={is_dead})")

    # Send response back to combat manager (Action ID 2002)
    response = {
        "action_id": 2002,
        "attacker_id": attacker_id,
        "target_id": target_id,
        "damage": final_dmg,
        "is_crit": is_crit,
        "is_dead": is_dead
    }
    await send_response(writer, response)

async def handle_chat(packet, writer):
    sender_name = packet.get("sender_name")
    channel = packet.get("chat_channel")
    message = packet.get("message")
    print(f"[CHAT] [{channel}] {sender_name}: {message}")

    # Broadcast message packet back to all clients (Action ID 2005)
    response = {
        "action_id": 2005,
        "sender_id": packet.get("sender_id"),
        "sender_name": sender_name,
        "chat_channel": channel,
        "message": message,
        "timestamp": int(time.time())
    }
    await broadcast(json.dumps(response))

async def handle_quest(packet, writer):
    char_id = packet.get("character_id", 1024)
    quest_id = packet.get("quest_id")
    status = packet.get("status")
    count = packet.get("progress_count", 0)
    
    print(f"[QUEST REQUEST] Char={char_id}, Quest={quest_id}, Status={status}, Count={count}")
    
    # Initialize quest log if not present
    if char_id not in character_quests:
        character_quests[char_id] = {}
        
    char_quests = character_quests[char_id]
    
    # Check if quest was already completed to prevent double-claiming rewards
    already_completed = char_quests.get(quest_id, {}).get("status") == "COMPLETED"
    
    # Update progress
    char_quests[quest_id] = {
        "status": status,
        "progress": count
    }
    
    # Retrieve quest config details
    quest_config = quests_db.get(quest_id)
    if not quest_config:
        print(f"[WARNING] Quest ID {quest_id} not found in quests database!")
        return
        
    message = f"Tiến trình nhiệm vụ: {quest_config['name']} ({count}/{quest_config['target_count']})"
    
    # Handle completion and distribute rewards
    if status == "COMPLETED" and not already_completed:
        exp_reward = quest_config.get("reward_exp", 0)
        gold_reward = quest_config.get("reward_gold", 0)
        
        # Add rewards to player_stats
        stats = player_stats.get(char_id)
        if stats:
            if "exp" not in stats:
                stats["exp"] = 0
            if "gold" not in stats:
                stats["gold"] = 0
                
            stats["gold"] += gold_reward
            stats["exp"] += exp_reward
            
            message = f"Nhiệm vụ [{quest_config['name']}] Hoàn Thành! Nhận: +{exp_reward} EXP, +{gold_reward} Vàng."
            print(f"[QUEST COMPLETED] Char {char_id} completed {quest_config['name']}. Gained {exp_reward} EXP, {gold_reward} Gold.")
            
            # Level up calculation
            leveled_up = False
            while True:
                next_level_exp = stats["level"] * 120 # Leveling curve formula
                if stats["exp"] >= next_level_exp:
                    stats["exp"] -= next_level_exp
                    stats["level"] += 1
                    stats["hp_max"] = int(stats["hp_max"] * 1.1)
                    stats["hp"] = stats["hp_max"]
                    stats["attack"] += 2
                    stats["defense"] += 1
                    leveled_up = True
                    print(f"[LEVEL UP] Char {char_id} leveled up to {stats['level']}!")
                else:
                    break
                    
            if leveled_up:
                message += f" Chúc mừng đại hiệp thăng lên cấp {stats['level']}!"
                
            # Send sync response (Action ID 2008)
            response = {
                "action_id": 2008,
                "quest_id": quest_id,
                "status": "COMPLETED",
                "name": stats["name"],
                "level": stats["level"],
                "hp": stats["hp"],
                "hp_max": stats["hp_max"],
                "gold": stats["gold"],
                "message": message
            }
            await send_response(writer, response)
            return
            
    # Send simple sync response if in-progress
    stats = player_stats.get(char_id, {})
    response = {
        "action_id": 2008,
        "quest_id": quest_id,
        "status": status,
        "name": stats.get("name", "Unknown"),
        "level": stats.get("level", 1),
        "hp": stats.get("hp", 100),
        "hp_max": stats.get("hp_max", 100),
        "gold": stats.get("gold", 0),
        "message": message
    }
    await send_response(writer, response)

async def handle_mount(packet, writer):
    mount_id = packet.get("mount_id")
    state = packet.get("is_equipped")
    print(f"[MOUNT] Player changed mount {mount_id} state to Equipped={state}")

async def handle_pet(packet, writer):
    pet_id = packet.get("pet_id")
    state = packet.get("is_summoned")
    print(f"[PET] Player changed pet {pet_id} state to Summoned={state}")

async def main():
    server = await asyncio.start_server(handle_client, HOST, PORT)
    addr = server.sockets[0].getsockname()
    print(f"[START] Tay Du Ky Mobile Python Asyncio Server listening on {addr}")
    
    async with server:
        await server.serve_forever()

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("[SHUTDOWN] Stopping server...")
