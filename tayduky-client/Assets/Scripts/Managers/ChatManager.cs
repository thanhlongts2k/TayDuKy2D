using System;
using UnityEngine;
using TayDuKy.Network;
using TayDuKy.UI;

namespace TayDuKy.Managers
{
    public class ChatManager : MonoBehaviour
    {
        public static ChatManager Instance { get; private set; }

        [System.Serializable]
        public class ChatPacket
        {
            public int action_id;
            public int sender_id;
            public string sender_name;
            public string chat_channel; // "WORLD", "GUILD", "WHISPER"
            public string message;
            public long timestamp;
        }

        public int CharacterId { get; set; } = 1024;
        public string CharacterName { get; set; } = "shinichi";

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

        public void SendWorldChat(string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            // Prepare JSON payload for Action ID 1005
            string chatPayload = $"{{\"action_id\": 1005, \"sender_id\": {CharacterId}, \"sender_name\": \"{CharacterName}\", \"chat_channel\": \"WORLD\", \"message\": \"{content}\", \"timestamp\": {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}}}";

            if (NetworkClient.Instance != null)
            {
                NetworkClient.Instance.SendPacket(chatPayload);
                Debug.Log($"Chat: Sent world chat message -> {content}");
            }
        }

        public void OnReceiveChatPacket(string jsonPayload)
        {
            try
            {
                ChatPacket packet = JsonUtility.FromJson<ChatPacket>(jsonPayload);
                
                // Route message to UI Chat History window
                if (UIManager.Instance != null)
                {
                    string channelPrefix = packet.chat_channel == "WORLD" ? "[Thế Giới]" : "[Bang]";
                    UIManager.Instance.AppendChatMessage($"{channelPrefix} {packet.sender_name}", packet.message);
                }
                
                Debug.Log($"Chat: Received chat from {packet.sender_name}: {packet.message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing chat packet: {ex.Message}");
            }
        }
    }
}
