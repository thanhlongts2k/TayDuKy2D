using UnityEngine;

namespace TayDuKy.UI
{
    /// <summary>
    /// Utility component: gắn vào nút X để đóng một panel cụ thể.
    /// Dùng trong SetupTestScene để AddPersistentListener không cần lambda.
    /// </summary>
    public class PanelCloser : MonoBehaviour
    {
        [Tooltip("Panel sẽ bị ẩn khi nhấn nút này.")]
        public GameObject targetPanel;

        public void ClosePanel()
        {
            if (targetPanel != null)
                targetPanel.SetActive(false);
        }
    }
}
