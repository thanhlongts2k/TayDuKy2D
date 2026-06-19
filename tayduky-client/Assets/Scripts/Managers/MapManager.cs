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
                LoadMap(101);
            }
        }

        private void Update()
        {
            if (activeMap == null) return;

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && !player.IsMoving)
            {
                Vector3 playerPos = player.transform.position;
                PortalLayout nearPortal = null;

                foreach (var portal in activeMap.portals)
                {
                    // Chebyshev distance: Max(|dx|, |dy|)
                    float dist = Mathf.Max(Mathf.Abs(playerPos.x - portal.x), Mathf.Abs(playerPos.y - portal.y));
                    if (dist <= portal.trigger_distance)
                    {
                        nearPortal = portal;
                        break;
                    }
                }

                if (nearPortal != null)
                {
                    if (activeTrackingPortal != nearPortal)
                    {
                        activeTrackingPortal = nearPortal;
                        portalStandTimer = 0f;
                        Debug.Log($"MapManager: Player is near portal ({nearPortal.x}, {nearPortal.y}). Starting standing timer (Target: {nearPortal.stand_time}s).");
                    }

                    portalStandTimer += Time.deltaTime;
                    if (portalStandTimer >= nearPortal.stand_time)
                    {
                        Debug.Log($"MapManager: Standing timer reached {nearPortal.stand_time}s. Triggering teleport to map {nearPortal.target_map_id}.");
                        int targetMapId = nearPortal.target_map_id;
                        Vector3 targetPos = new Vector3(nearPortal.target_x, nearPortal.target_y, 0f);

                        // Reset tracking first to avoid double triggers before load completes
                        activeTrackingPortal = null;
                        portalStandTimer = 0f;

                        LoadMap(targetMapId);
                        player.TeleportTo(targetPos, targetMapId);
                    }
                }
                else
                {
                    if (activeTrackingPortal != null)
                    {
                        Debug.Log("MapManager: Player left portal area. Resetting standing timer.");
                        activeTrackingPortal = null;
                        portalStandTimer = 0f;
                    }
                }
            }
            else
            {
                if (activeTrackingPortal != null)
                {
                    activeTrackingPortal = null;
                    portalStandTimer = 0f;
                }
            }
        }

        public void LoadMap(int mapId)
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
            if (bgSprite != null)
            {
                sr.sprite = bgSprite;
                sr.sortingOrder = -10; // Draw behind players and NPCs

                // Position background centered in the 24x24 grid (center is (11.5, 11.5))
                mapBackgroundObj.transform.position = new Vector3(11.5f, 11.5f, 10.0f);
                
                // Scale sprite to fit the 24x24 grid bounds
                float spriteW = bgSprite.bounds.size.x;
                float spriteH = bgSprite.bounds.size.y;
                mapBackgroundObj.transform.localScale = new Vector3(24.0f / spriteW, 24.0f / spriteH, 1.0f);
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
                npcSr.sortingOrder = 2; // Behind player UI but on top of map

                // Position on grid cell center
                npcObj.transform.position = new Vector3(npcConfig.x, npcConfig.y, 0f);

                // Assign sprite dynamically from PlayerController faction sheets
                if (player != null)
                {
                    npcSr.sprite = player.GetFactionDefaultSprite(npcConfig.sprite_resource);
                }

                // Add NPC tag or component if dialogues/quest interaction is added later
                activeNPCs.Add(npcObj);
            }

            Debug.Log($"MapManager: Spawned {activeMap.npcs.Count} NPCs on map {activeMap.name}");
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

                    // For prototype: we assume faction name is determined by sprite configuration or default Thần Tộc
                    string factionName = "Thần Tộc";
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
    }
}
