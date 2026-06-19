using System;
using System.Collections.Generic;
using UnityEngine;
using TayDuKy.Network;

namespace TayDuKy.Managers
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [System.Serializable]
        public class ActiveQuest
        {
            public int questId;
            public string name;
            public int currentCount;
            public int targetCount;
            public string targetName;
            public bool isCompleted;
        }

        public List<ActiveQuest> activeQuestsList = new List<ActiveQuest>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AcceptQuest(int questId)
        {
            if (ConfigManager.Instance == null) return;

            ConfigManager.QuestConfig config = ConfigManager.Instance.GetQuestById(questId);
            if (config == null) return;

            // Check if already accepted
            if (activeQuestsList.Exists(x => x.questId == questId))
            {
                Debug.LogWarning($"Quest: Quest {config.name} already accepted.");
                return;
            }

            ActiveQuest newQuest = new ActiveQuest
            {
                questId = config.id,
                name = config.name,
                currentCount = 0,
                targetCount = config.target_count,
                targetName = config.target_name,
                isCompleted = false
            };

            activeQuestsList.Add(newQuest);
            Debug.Log($"Quest: Accepted quest -> {newQuest.name}. Target: Kill {newQuest.targetCount} {newQuest.targetName}");

            // Notify server that player accepted quest (Action ID 1008)
            SendQuestStatusPacket(questId, "IN_PROGRESS", 0);
        }

        // Call this whenever a monster is killed or item is gathered
        public void OnTargetDefeated(string targetName)
        {
            foreach (var quest in activeQuestsList)
            {
                if (!quest.isCompleted && quest.targetName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    quest.currentCount++;
                    Debug.Log($"Quest '{quest.name}' progress: {quest.currentCount}/{quest.targetCount}");

                    if (quest.currentCount >= quest.targetCount)
                    {
                        quest.currentCount = quest.targetCount;
                        quest.isCompleted = true;
                        Debug.Log($"Quest '{quest.name}' is READY for submission!");
                    }

                    // Sync progress with Server Go (Action ID 1008)
                    SendQuestStatusPacket(quest.questId, quest.isCompleted ? "COMPLETED" : "IN_PROGRESS", quest.currentCount);
                }
            }
        }

        public void InteractWithQuestNPC(string npcName)
        {
            // Check if there are any completed quests associated with this NPC to turn in
            for (int i = activeQuestsList.Count - 1; i >= 0; i--)
            {
                var quest = activeQuestsList[i];
                if (quest.isCompleted)
                {
                    ConfigManager.QuestConfig config = ConfigManager.Instance.GetQuestById(quest.questId);
                    if (config != null && config.npc_giver.Equals(npcName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Complete and turn in quest
                        Debug.Log($"Quest: Turned in quest -> {quest.name} to NPC {npcName}. Rewards: +{config.reward_exp} EXP, +{config.reward_gold} Gold");
                        
                        // Notify server (Action ID 1008 - Status: COMPLETED)
                        SendQuestStatusPacket(quest.questId, "COMPLETED", quest.currentCount);

                        activeQuestsList.RemoveAt(i);
                    }
                }
            }
        }

        private void SendQuestStatusPacket(int questId, string status, int currentCount)
        {
            if (NetworkClient.Instance == null) return;

            int charId = 1024;
            var player = FindFirstObjectByType<TayDuKy.Controllers.PlayerController>();
            if (player != null)
            {
                charId = player.CharacterId;
            }

            // Action ID 1008 representing Quest Sync
            string questPayload = $"{{\"action_id\": 1008, \"character_id\": {charId}, \"quest_id\": {questId}, \"status\": \"{status}\", \"progress_count\": {currentCount}}}";
            
            NetworkClient.Instance.SendPacket(questPayload);
        }
    }
}
