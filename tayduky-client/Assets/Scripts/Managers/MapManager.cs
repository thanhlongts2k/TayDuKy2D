using System;
using System.Collections.Generic;
using UnityEngine;
using TayDuKy.Controllers;

namespace TayDuKy.Managers
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [System.Serializable]
        public class ObstacleCoordinate
        {
            public int x;
            public int y;
        }

        [System.Serializable]
        public class NPCLayout
        {
            public int id;
            public string name;
            public int x;
            public int y;
            public string dialogue;
            public string sprite_resource;
        }

        [System.Serializable]
        public class PortalLayout
        {
            public int x;
            public int y;
            public int target_map_id;
            public float target_x;
            public float target_y;
            public float stand_time = 1.5f;
            public float trigger_distance = 1.0f;
        }

        [System.Serializable]
        public class MapConfig
        {
            public int id;
            public string name;
            public int width;
            public int height;
            public float spawn_x;
            public float spawn_y;
            public string bg_resource_path;
            public List<ObstacleCoordinate> obstacles;
            public List<NPCLayout> npcs;
            public List<PortalLayout> portals;
        }

        [System.Serializable]
        private class MapListWrapper { public List<MapConfig> maps; }

        public List<MapConfig> mapsList = new List<MapConfig>();
        private MapConfig activeMap = null;
        private GameObject mapBackgroundObj = null;
        private List<GameObject> activeNPCs = new List<GameObject>();
        private float portalStandTimer = 0f;
        private PortalLayout activeTrackingPortal = null;
        private float teleportCooldownTimer = 0f;
        private Sprite portalDotSprite = null;

        public int ActiveMapId => activeMap != null ? activeMap.id : 101;
        public string ActiveMapName => activeMap != null ? activeMap.name : "Hội Bàn Đào";

        private Dictionary<int, OtherPlayerController> otherPlayers = new Dictionary<int, OtherPlayerController>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadMapConfigurations();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadMapConfigurations()
        {
            try
            {
                TextAsset mapsText = Resources.Load<TextAsset>("Maps/maps");
                if (mapsText != null)
                {
                    string wrappedJson = $"{{\"maps\":{mapsText.text}}}";
                    MapListWrapper wrapper = JsonUtility.FromJson<MapListWrapper>(wrappedJson);
                    mapsList = wrapper.maps;
                    Debug.Log($"MapManager: Loaded {mapsList.Count} maps configurations successfully.");
                }
                else
                {
                    Debug.LogWarning("MapManager: maps.json not found in Resources/Maps/");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"MapManager: Failed to parse maps.json: {e.Message}");
            }
        }

        private void Start()
        {
            // Load the default starter map at start if not loaded
            if (activeMap == null && mapsList.Count > 0)
            {
                // spawnPlayer=true: teleport player to map spawn point on initial load
                LoadMap(101, spawnPlayer: true);
            }
        }

        private void Update()
        {
            if (activeMap == null) return;

            if (teleportCooldownTimer > 0f)
            {
                teleportCooldownTimer -= Time.deltaTime;
            }

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && teleportCooldownTimer <= 0f)
            {
                Vector3 playerPos = player.transform.position;
                PortalLayout nearPortal = null;

                foreach (var portal in activeMap.portals)
                {
                    // Check if player position is very close to portal (close overlap)
                    float dist = Vector3.Distance(new Vector3(playerPos.x, playerPos.y, 0f), new Vector3(portal.x, portal.y, 0f));
                    if (dist <= portal.trigger_distance)
                    {
                        nearPortal = portal;
                        break;
                    }
                }

                if (nearPortal != null)
                {
                    Debug.Log($"MapManager: Player closely overlapped portal ({nearPortal.x}, {nearPortal.y}). Triggering instant teleport to map {nearPortal.target_map_id}.");
                    int targetMapId = nearPortal.target_map_id;
                    Vector3 targetPos = new Vector3(nearPortal.target_x, nearPortal.target_y, 0f);

                    // Set teleport cooldown to prevent double-triggering when spawning
                    teleportCooldownTimer = 1.0f;

                    LoadMap(targetMapId, spawnPlayer: false);
                    player.TeleportTo(targetPos, targetMapId);
                }
            }
        }

        public void LoadMap(int mapId, bool spawnPlayer = false)
        {
            MapConfig config = mapsList.Find(x => x.id == mapId);
            if (config == null)
            {
                Debug.LogError($"MapManager: Map ID {mapId} not found in configurations!");
                return;
            }

            activeMap = config;
            Debug.Log($"MapManager: Loading map '{activeMap.name}' (ID: {mapId})");

            // 1. Setup background graphic
            if (mapBackgroundObj == null)
            {
                mapBackgroundObj = new GameObject("MapBackground");
                mapBackgroundObj.transform.SetParent(transform);
            }

            var sr = mapBackgroundObj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = mapBackgroundObj.AddComponent<SpriteRenderer>();

            Sprite bgSprite = Resources.Load<Sprite>(activeMap.bg_resource_path);

            // Fallback: map PNGs are often imported as Texture2D (not Sprite).
            // Load as Texture2D and wrap into a Sprite at runtime so background always shows.
            if (bgSprite == null)
            {
                Texture2D bgTex = Resources.Load<Texture2D>(activeMap.bg_resource_path);
                if (bgTex != null)
                {
                    bgSprite = Sprite.Create(
                        bgTex,
                        new Rect(0, 0, bgTex.width, bgTex.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                    Debug.Log($"MapManager: Loaded background '{activeMap.bg_resource_path}' via Texture2D fallback.");
                }
            }
            if (bgSprite != null)
            {
                sr.sprite = bgSprite;
                sr.sortingOrder = -10; // Draw behind players and NPCs

                // BUG FIX #1: Z must be 0 in 2D – Z=10 pushed background behind Camera (at Z=-10)
                float centerX = activeMap.width * 0.5f;
                float centerY = activeMap.height * 0.5f;
                mapBackgroundObj.transform.position = new Vector3(centerX, centerY, 0f);

                // Scale sprite to fit the map grid bounds exactly
                float spriteW = bgSprite.bounds.size.x;
                float spriteH = bgSprite.bounds.size.y;
                mapBackgroundObj.transform.localScale = new Vector3(
                    (float)activeMap.width  / spriteW,
                    (float)activeMap.height / spriteH,
                    1.0f);

                Debug.Log($"MapManager: Background set – Center=({centerX},{centerY}), Scale=({(float)activeMap.width/spriteW:F2},{(float)activeMap.height/spriteH:F2})");
            }
            else
            {
                Debug.LogError($"MapManager: Failed to load background sprite from: Resources/{activeMap.bg_resource_path}");
            }

            // 2. Clear old NPCs and other players
            foreach (var npc in activeNPCs)
            {
                if (npc != null) Destroy(npc);
            }
            activeNPCs.Clear();

            foreach (var op in otherPlayers.Values)
            {
                if (op != null) Destroy(op.gameObject);
            }
            otherPlayers.Clear();

            // 3. Spawn map NPCs
            PlayerController player = FindFirstObjectByType<PlayerController>();
            foreach (var npcConfig in activeMap.npcs)
            {
                GameObject npcObj = new GameObject("NPC_" + npcConfig.name);
                var npcSr = npcObj.AddComponent<SpriteRenderer>();
                npcSr.sortingOrder = 2; // Above map background, below player UI

                // Position on grid cell center, Z=0 to stay in 2D plane
                npcObj.transform.position = new Vector3(npcConfig.x, npcConfig.y, 0f);

                // BUG FIX #5: Null-safe sprite assignment from PlayerController faction sheets
                if (player != null)
                {
                    Sprite npcSprite = player.GetFactionDefaultSprite(npcConfig.sprite_resource);
                    if (npcSprite != null)
                    {
                        npcSr.sprite = npcSprite;
                    }
                    else
                    {
                        Debug.LogWarning($"MapManager: Sprite not found for NPC '{npcConfig.name}' using resource key '{npcConfig.sprite_resource}'. NPC will be invisible.");
                    }
                }

                // Add NPC tag or component if dialogues/quest interaction is added later
                activeNPCs.Add(npcObj);
            }

            Debug.Log($"MapManager: Spawned {activeMap.npcs.Count} NPCs on map {activeMap.name}");

            // 3.5. Spawn portal dot indicators (small dots on ground)
            Sprite dotSprite = GetPortalDotSprite();
            foreach (var portal in activeMap.portals)
            {
                GameObject portalVisual = new GameObject($"Portal_Dot_To_{portal.target_map_id}");
                portalVisual.transform.SetParent(transform);
                var pSr = portalVisual.AddComponent<SpriteRenderer>();
                pSr.sprite = dotSprite;
                pSr.color = new Color(0f, 0.9f, 1f, 0.7f); // Bright semi-transparent cyan
                pSr.sortingOrder = -1; // Under the players' feet, above background

                // Position portal dot on the ground
                portalVisual.transform.position = new Vector3(portal.x, portal.y, 0f);
                portalVisual.transform.localScale = new Vector3(0.35f, 0.35f, 1f); // Neat small dot
                portalVisual.AddComponent<TayDuKy.UI.PortalDot>(); // Add pulse animation effect

                activeNPCs.Add(portalVisual); // Automatically cleaned up on map unload
            }

            // BUG FIX #2: Spawn player at map spawn point if requested
            if (spawnPlayer)
            {
                PlayerController spawnTarget = player ?? FindFirstObjectByType<PlayerController>();
                if (spawnTarget != null)
                {
                    Vector3 spawnPos = new Vector3(activeMap.spawn_x, activeMap.spawn_y, 0f);
                    spawnTarget.TeleportTo(spawnPos, mapId);
                    Debug.Log($"MapManager: Spawned player at ({activeMap.spawn_x}, {activeMap.spawn_y}) on map '{activeMap.name}'.");
                }
                else
                {
                    Debug.LogWarning("MapManager: spawnPlayer=true but no PlayerController found in scene!");
                }
            }

            // Update camera bounds to match new map size
            var camFollow = Camera.main != null ? Camera.main.GetComponent<TayDuKy.Controllers.CameraFollow>() : null;
            if (camFollow != null)
            {
                camFollow.SetMapBounds(activeMap.width, activeMap.height);
            }
        }

        public bool CanWalk(Vector3 worldPos)
        {
            if (activeMap == null) return true;

            int x = Mathf.RoundToInt(worldPos.x);
            int y = Mathf.RoundToInt(worldPos.y);

            // 1. Boundary check
            if (x < 0 || x >= activeMap.width || y < 0 || y >= activeMap.height)
            {
                return false;
            }

            // 2. Obstacles check
            foreach (var obs in activeMap.obstacles)
            {
                if (obs.x == x && obs.y == y) return false;
            }

            // 3. NPCs block check
            foreach (var npc in activeMap.npcs)
            {
                if (npc.x == x && npc.y == y) return false;
            }

            return true;
        }

        [System.Serializable]
        public class MoveEventResponse
        {
            public int action_id;
            public int character_id;
            public int map_id;
            public string name;
            public string faction;
            public float current_x;
            public float current_y;
            public string direction;
            public string mount_type;
            public long timestamp;
        }

        public void OnOtherPlayerMoved(string jsonPayload)
        {
            try
            {
                MoveEventResponse response = JsonUtility.FromJson<MoveEventResponse>(jsonPayload);
                if (response == null) return;

                // 1. Check if they moved to a different map
                if (response.map_id != ActiveMapId)
                {
                    if (otherPlayers.ContainsKey(response.character_id))
                    {
                        Destroy(otherPlayers[response.character_id].gameObject);
                        otherPlayers.Remove(response.character_id);
                    }
                    return;
                }

                // 2. Skip if it is local player
                PlayerController localPlayer = FindFirstObjectByType<PlayerController>();
                if (localPlayer != null && response.character_id == localPlayer.CharacterId)
                {
                    return;
                }

                Vector3 targetPos = new Vector3(response.current_x, response.current_y, 0f);

                // 3. Spawn other player if they are not tracked yet
                if (!otherPlayers.ContainsKey(response.character_id))
                {
                    GameObject obj = new GameObject($"OtherPlayer_{response.name}_{response.character_id}");
                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 3; // Render same layer as player

                    var otherCtrl = obj.AddComponent<OtherPlayerController>();
                    if (localPlayer != null)
                    {
                        otherCtrl.SetSpriteSheets(localPlayer.ThanTocSprites, localPlayer.MaTocSprites, localPlayer.YeuTocSprites);
                    }

                    // Use the faction sent by the server (falls back to Thần Tộc if missing)
                    string factionName = !string.IsNullOrEmpty(response.faction) ? response.faction : "Thần Tộc";
                    otherCtrl.SetCharacter(response.character_id, response.name, factionName);
                    otherCtrl.TeleportTo(targetPos);
                    otherPlayers[response.character_id] = otherCtrl;
                    
                    Debug.Log($"MapManager: Spawned new player '{response.name}' (ID: {response.character_id}) at ({response.current_x}, {response.current_y})");
                }
                else
                {
                    // 4. Move existing player
                    otherPlayers[response.character_id].MoveTo(targetPos, response.direction);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error on other player move: {ex.Message}");
            }
        }

        private Sprite GetPortalDotSprite()
        {
            if (portalDotSprite != null) return portalDotSprite;

            // Load built-in circle or create one
            portalDotSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            if (portalDotSprite == null)
            {
                Texture2D tex = new Texture2D(32, 32);
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float dx = x - 15.5f;
                        float dy = y - 15.5f;
                        if (dx * dx + dy * dy <= 15f * 15f)
                            tex.SetPixel(x, y, Color.white);
                        else
                            tex.SetPixel(x, y, Color.clear);
                    }
                }
                tex.Apply();
                portalDotSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);
            }
            return portalDotSprite;
        }
    }
}
