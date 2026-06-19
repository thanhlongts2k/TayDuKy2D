using System;
using UnityEngine;
using UnityEngine.UI;
using TayDuKy.Network;

namespace TayDuKy.UI
{
    public class LoginManager : MonoBehaviour
    {
        public static LoginManager Instance { get; private set; }

        [Header("UI Inputs")]
        [SerializeField] private InputField usernameInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Text statusText;

        [Header("UI Buttons")]
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClick);

            if (registerButton != null)
                registerButton.onClick.AddListener(OnRegisterClick);

            ShowStatus("Nhập tài khoản để bắt đầu phiêu lưu!");
        }

        public void OnLoginClick()
        {
            if (usernameInput == null || passwordInput == null) return;

            string username = usernameInput.text.Trim();
            string password = passwordInput.text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowStatus("Tài khoản và mật khẩu không được để trống!");
                return;
            }

            ShowStatus("Đang kết nối đến máy chủ...");

            // Bắt đầu kết nối mạng nếu chưa kết nối
            if (NetworkClient.Instance != null)
            {
                NetworkClient.Instance.LoggedInUsername = username;
                // Gửi gói tin Login (Action ID 1000)
                string loginPayload = "{\"action_id\": 1000, \"username\": \"" + username + "\", \"password\": \"" + password + "\"}";
                NetworkClient.Instance.SendPacket(loginPayload);
                Debug.Log($"Client sent login request: User={username}");
            }
            else
            {
                ShowStatus("Lỗi kết nối mạng: Không tìm thấy NetworkClient!");
            }
        }

        public void OnRegisterClick()
        {
            if (usernameInput == null || passwordInput == null) return;

            string username = usernameInput.text.Trim();
            string password = passwordInput.text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowStatus("Tài khoản và mật khẩu không được để trống!");
                return;
            }

            ShowStatus("Đang đăng ký tài khoản mới...");

            if (NetworkClient.Instance != null)
            {
                NetworkClient.Instance.LoggedInUsername = username;
                // Đăng ký (Action ID 1004)
                string registerPayload = "{\"action_id\": 1004, \"username\": \"" + username + "\", \"password\": \"" + password + "\"}";
                NetworkClient.Instance.SendPacket(registerPayload);
                Debug.Log($"Client sent register request: User={username}");
            }
            else
            {
                ShowStatus("Lỗi kết nối mạng!");
            }
        }

        public void ShowStatus(string msg, bool isError = false)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = isError ? Color.red : Color.yellow;
        }
    }
}
