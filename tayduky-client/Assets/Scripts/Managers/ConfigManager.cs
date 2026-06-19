using System;
using System.Collections.Generic;
using UnityEngine;

namespace TayDuKy.Managers
{
    public class ConfigManager : MonoBehaviour
    {
        public static ConfigManager Instance { get; private set; }

        [System.Serializable]
        public class ItemConfig
        {
            public int id;
            public string name;
            public string type;
            public int required_level;
            public int attack_bonus;
            public string description;
        }

        [System.Serializable]
        public class QuestConfig
        {
            public int id;
            public string name;
            public int min_level;
            public string npc_giver;
            public int target_count;
            public string target_name;
            public int reward_exp;
            public int reward_gold;
        }

        [System.Serializable]
        public class CompanionConfig
        {
            public int id;
            public string name;
            public string rarity;
            public string faction;
            public string attack_type;
            public int base_atk;
            public int base_def;
            public int base_hp;
            public string description;
        }

        // Wrapper lists for parsing array JSON in Unity JsonUtility
        private class ItemListWrapper { public List<ItemConfig> items; }
        private class QuestListWrapper { public List<QuestConfig> quests; }
        private class CompanionListWrapper { public List<CompanionConfig> companions; }

        public List<ItemConfig> ItemsList { get; private set; } = new List<ItemConfig>();
        public List<QuestConfig> QuestsList { get; private set; } = new List<QuestConfig>();
        public List<CompanionConfig> CompanionsList { get; private set; } = new List<CompanionConfig>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadGameConfigs();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadGameConfigs()
        {
            try
            {
                // Load items.json from Assets/Resources/Items/items.json
                TextAsset itemsText = Resources.Load<TextAsset>("Items/items");
                if (itemsText != null)
                {
                    // Since Unity JsonUtility doesn't support direct arrays well, we wrap it
                    string wrappedItems = $"{{\"items\":{itemsText.text}}}";
                    ItemListWrapper wrapper = JsonUtility.FromJson<ItemListWrapper>(wrappedItems);
                    ItemsList = wrapper.items;
                    Debug.Log($"Successfully loaded {ItemsList.Count} items configurations.");
                }
                else
                {
                    Debug.LogWarning("items.json not found in Resources/Items/");
                }

                // Load quests.json from Assets/Resources/Quests/quests.json
                TextAsset questsText = Resources.Load<TextAsset>("Quests/quests");
                if (questsText != null)
                {
                    string wrappedQuests = $"{{\"quests\":{questsText.text}}}";
                    QuestListWrapper wrapper = JsonUtility.FromJson<QuestListWrapper>(wrappedQuests);
                    QuestsList = wrapper.quests;
                    Debug.Log($"Successfully loaded {QuestsList.Count} quests configurations.");
                }
                else
                {
                    Debug.LogWarning("quests.json not found in Resources/Quests/");
                }

                // Load companions.json from Assets/Resources/Companions/companions.json
                TextAsset companionsText = Resources.Load<TextAsset>("Companions/companions");
                if (companionsText != null)
                {
                    string wrappedCompanions = $"{{\"companions\":{companionsText.text}}}";
                    CompanionListWrapper wrapper = JsonUtility.FromJson<CompanionListWrapper>(wrappedCompanions);
                    CompanionsList = wrapper.companions;
                    Debug.Log($"Successfully loaded {CompanionsList.Count} companions configurations.");
                }
                else
                {
                    Debug.LogWarning("companions.json not found in Resources/Companions/");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading game configuration files: {ex.Message}");
            }
        }

        public ItemConfig GetItemById(int id)
        {
            return ItemsList.Find(x => x.id == id);
        }

        public QuestConfig GetQuestById(int id)
        {
            return QuestsList.Find(x => x.id == id);
        }

        public CompanionConfig GetCompanionById(int id)
        {
            return CompanionsList.Find(x => x.id == id);
        }
    }
}
