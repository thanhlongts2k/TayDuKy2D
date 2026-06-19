using System;
using UnityEngine;
using TayDuKy.Network;

namespace TayDuKy.Managers
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [System.Serializable]
        public class CombatActor
        {
            public int id;
            public string name;
            public string faction;
            public int level;
            public int hpCurrent;
            public int hpMax;
        }

        [System.Serializable]
        public class AttackResult
        {
            public int action_id;
            public int attacker_id;
            public int target_id;
            public int damage;
            public bool is_crit;
            public bool is_dead;
        }

        public bool isInCombat { get; private set; } = false;
        public CombatActor playerActor;
        public CombatActor enemyActor;

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

        public void StartCombat(CombatActor player, CombatActor enemy)
        {
            playerActor = player;
            enemyActor = enemy;
            isInCombat = true;
            Debug.Log($"Entered turn-based combat with {enemy.name} (Level {enemy.level})");
            // Trigger UI transitions here
        }

        public void SendAttackCommand()
        {
            if (!isInCombat) return;

            // Send attack packet to Server
            // Action ID 1002 representing combat attack trigger
            string attackPayload = $"{{\"action_id\": 1002, \"attacker_id\": {playerActor.id}, \"target_id\": {enemyActor.id}}}";
            
            if (NetworkClient.Instance != null)
            {
                NetworkClient.Instance.SendPacket(attackPayload);
                Debug.Log("Client sent attack action command.");
            }
        }

        public void OnReceiveAttackResult(string jsonResult)
        {
            try
            {
                AttackResult result = JsonUtility.FromJson<AttackResult>(jsonResult);
                
                // Update actor HP in local registry
                if (result.target_id == playerActor.id)
                {
                    playerActor.hpCurrent -= result.damage;
                    if (playerActor.hpCurrent < 0) playerActor.hpCurrent = 0;
                    Debug.Log($"Player received {result.damage} damage! Remaining HP: {playerActor.hpCurrent}");
                }
                else if (result.target_id == enemyActor.id)
                {
                    enemyActor.hpCurrent -= result.damage;
                    if (enemyActor.hpCurrent < 0) enemyActor.hpCurrent = 0;
                    Debug.Log($"Enemy received {result.damage} damage! Remaining HP: {enemyActor.hpCurrent}");
                }

                // Trigger visual hits, crit text, and update UI health bars
                if (result.is_crit)
                {
                    Debug.Log("CRITICAL HIT!");
                }

                if (result.is_dead)
                {
                    EndCombat(result.target_id == enemyActor.id);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing combat result: {ex.Message}");
            }
        }

        public void EndCombat(bool didPlayerWin)
        {
            isInCombat = false;
            if (didPlayerWin)
            {
                Debug.Log("Victory! Player defeated the monster.");
                // Award loot, gold, and exp
            }
            else
            {
                Debug.Log("Defeat! Player died. Returning to town...");
                // Resurrect player
            }
            // Trigger UI transitions back to WorldMap
        }
    }
}
