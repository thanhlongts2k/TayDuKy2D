using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace TayDuKy.Network
{
    public class NetworkClient : MonoBehaviour
    {
        public static NetworkClient Instance { get; private set; }
        public string LoggedInUsername { get; set; }

        [Header("Connection Settings")]
        [SerializeField] private string serverIP = "127.0.0.1";
        [SerializeField] private int serverPort = 8080;

        private TcpClient socket;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected = false;

        private Queue<string> packetQueue = new Queue<string>();
        private object queueLock = new object();

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

        private void Start()
        {
            ConnectToServer();
        }

        public void ConnectToServer()
        {
            try
            {
                socket = new TcpClient();
                socket.Connect(serverIP, serverPort);
                stream = socket.GetStream();
                isConnected = true;
                
                receiveThread = new Thread(ReceiveData);
                receiveThread.IsBackground = true;
                receiveThread.Start();
                
                Debug.Log($"Connected to server at {serverIP}:{serverPort}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to connect to server: {ex.Message}");
            }
        }

        private void Update()
        {
            // Process received packets on the Main Unity Thread
            lock (queueLock)
            {
                while (packetQueue.Count > 0)
                {
                    string packetJson = packetQueue.Dequeue();
                    HandlePacket(packetJson);
                }
            }
        }

        private void ReceiveData()
        {
            byte[] buffer = new byte[4096];
            while (isConnected && socket != null && socket.Connected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead <= 0)
                    {
                        Disconnect();
                        break;
                    }

                    string receivedJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    lock (queueLock)
                    {
                        packetQueue.Enqueue(receivedJson);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Read error or socket disconnected: {ex.Message}");
                    Disconnect();
                    break;
                }
            }
        }

        public void SendPacket(string jsonPayload)
        {
            if (!isConnected || socket == null || !socket.Connected)
            {
                Debug.LogWarning("Cannot send packet. Not connected to server.");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(jsonPayload);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending packet: {ex.Message}");
            }
        }

        [System.Serializable]
        public class BaseResponse
        {
            public int action_id;
        }

        [System.Serializable]
        public class LoginResponse
        {
            public int action_id;
            public string status;
            public bool has_character;
            public int character_id;
            public string name;
            public int level;
            public int hp;
            public int hp_max;
            public string faction;
            public string message;
        }

        [System.Serializable]
        public class CreateCharacterResponse
        {
            public int action_id;
            public string status;
            public int character_id;
            public string name;
            public int level;
            public int hp;
            public int hp_max;
            public string faction;
            public string message;
        }

        [System.Serializable]
        public class QuestSyncResponse
        {
            public int action_id;
            public int quest_id;
            public string status;
            public string name;
            public int level;
            public int hp;
            public int hp_max;
            public int gold;
            public string message;
        }

        private void HandlePacket(string json)
        {
            Debug.Log($"Received packet from server: {json}");
            
            try
            {
                BaseResponse baseResp = JsonUtility.FromJson<BaseResponse>(json);
                if (baseResp == null) return;

                if (baseResp.action_id == 2000) // Login Response
                {
                    LoginResponse loginResp = JsonUtility.FromJson<LoginResponse>(json);
                    if (loginResp.status == "success")
                    {
                        if (loginResp.has_character)
                        {
                            // Cập nhật thông tin nhân vật cho PlayerController
                            var player = FindFirstObjectByType<TayDuKy.Controllers.PlayerController>();
                            if (player != null)
                            {
                                player.SetCharacter(loginResp.character_id, loginResp.faction);
                            }

                            // Lưu trữ thông số nhân vật và chuyển vào Game World
                            if (TayDuKy.UI.UIManager.Instance != null)
                            {
                                TayDuKy.UI.UIManager.Instance.UpdateCharacterStats(
                                    loginResp.name, loginResp.level, loginResp.hp, loginResp.hp_max, 50, 50
                                );
                                TayDuKy.UI.UIManager.Instance.ShowWorld();
                            }
                        }
                        else
                        {
                            // Chuyển sang màn hình tạo nhân vật
                            if (TayDuKy.UI.UIManager.Instance != null)
                            {
                                TayDuKy.UI.UIManager.Instance.ShowCharacterCreation();
                            }
                        }
                    }
                    else
                    {
                        if (TayDuKy.UI.LoginManager.Instance != null)
                        {
                            TayDuKy.UI.LoginManager.Instance.ShowStatus(loginResp.message, true);
                        }
                    }
                }
                else if (baseResp.action_id == 2003) // Create Character Response
                {
                    CreateCharacterResponse createResp = JsonUtility.FromJson<CreateCharacterResponse>(json);
                    if (createResp.status == "success")
                    {
                        // Cập nhật thông tin nhân vật cho PlayerController
                        var player = FindFirstObjectByType<TayDuKy.Controllers.PlayerController>();
                        if (player != null)
                        {
                            player.SetCharacter(createResp.character_id, createResp.faction);
                        }

                        if (TayDuKy.UI.UIManager.Instance != null)
                        {
                            TayDuKy.UI.UIManager.Instance.UpdateCharacterStats(
                                createResp.name, createResp.level, createResp.hp, createResp.hp_max, 50, 50
                            );
                            TayDuKy.UI.UIManager.Instance.ShowWorld();
                        }
                    }
                    else
                    {
                        // Hiển thị lỗi tạo nhân vật
                        var creator = FindFirstObjectByType<TayDuKy.UI.CharacterCreationManager>();
                        if (creator != null)
                        {
                            creator.ShowStatus(createResp.message, true);
                        }
                    }
                }
                else if (baseResp.action_id == 2008) // Quest Sync/Update Response
                {
                    QuestSyncResponse questResp = JsonUtility.FromJson<QuestSyncResponse>(json);
                    if (TayDuKy.UI.UIManager.Instance != null)
                    {
                        if (!string.IsNullOrEmpty(questResp.message))
                        {
                            TayDuKy.UI.UIManager.Instance.AppendChatMessage("Hệ thống", questResp.message);
                        }
                        
                        TayDuKy.UI.UIManager.Instance.UpdateCharacterStats(
                            questResp.name, questResp.level, questResp.hp, questResp.hp_max, 50, 50
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling packet: {ex.Message} \n Raw JSON: {json}");
            }
        }

        public void Disconnect()
        {
            if (!isConnected) return;

            isConnected = false;
            if (stream != null) stream.Close();
            if (socket != null) socket.Close();
            
            Debug.Log("Disconnected from server.");
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}
