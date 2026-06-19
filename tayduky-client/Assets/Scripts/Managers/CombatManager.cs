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
            
            if (TayDuKy.UI.UIManager.Instance != null)
            {
                TayDuKy.UI.UIManager.Instance.ShowCombat(true);
                TayDuKy.UI.UIManager.Instance.ClearCombatLog();
                TayDuKy.UI.UIManager.Instance.UpdateCombatStats(
                    player.name, player.hpCurrent, player.hpMax,
                    enemy.name, enemy.hpCurrent, enemy.hpMax
                );
                TayDuKy.UI.UIManager.Instance.LogCombatMessage($"Gặp gỡ yêu quái {enemy.name}! Trận chiến bắt đầu.");
            }
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
                
                string logMsg = "";
                
                // Update actor HP in local registry
                if (result.target_id == playerActor.id)
                {
                    playerActor.hpCurrent -= result.damage;
                    if (playerActor.hpCurrent < 0) playerActor.hpCurrent = 0;
                    
                    string critSuffix = result.is_crit ? " bạo kích!" : "";
                    logMsg = $"{enemyActor.name} gây {result.damage} sát thương{critSuffix} lên {playerActor.name}.";
                }
                else if (result.target_id == enemyActor.id)
                {
                    enemyActor.hpCurrent -= result.damage;
                    if (enemyActor.hpCurrent < 0) enemyActor.hpCurrent = 0;
                    
                    string critSuffix = result.is_crit ? " bạo kích!" : "";
                    logMsg = $"{playerActor.name} gây {result.damage} sát thương{critSuffix} lên {enemyActor.name}.";
                }

                if (TayDuKy.UI.UIManager.Instance != null)
                {
                    TayDuKy.UI.UIManager.Instance.LogCombatMessage(logMsg);
                    TayDuKy.UI.UIManager.Instance.UpdateCombatStats(
                        playerActor.name, playerActor.hpCurrent, playerActor.hpMax,
                        enemyActor.name, enemyActor.hpCurrent, enemyActor.hpMax
                    );
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
            string finalMsg = "";
            if (didPlayerWin)
            {
                finalMsg = "Chiến thắng! Tiêu diệt yêu quái thành công.";
                Debug.Log(finalMsg);
            }
            else
            {
                finalMsg = "Thất bại! Sức cùng lực kiệt, hãy quay lại dưỡng thương.";
                Debug.Log(finalMsg);
            }

            if (TayDuKy.UI.UIManager.Instance != null)
            {
                TayDuKy.UI.UIManager.Instance.LogCombatMessage(finalMsg);
                // Return to world map after a small delay
                TayDuKy.UI.UIManager.Instance.Invoke("ShowWorld", 1.5f);
            }
        }
    }
}
