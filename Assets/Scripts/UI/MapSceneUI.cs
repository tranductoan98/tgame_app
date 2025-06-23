using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSceneUI : MonoBehaviour
{
    public TextMeshProUGUI mapNameText;

    public void SetMapInfo(string name)
    {
        mapNameText.gameObject.SetActive(false);
        if (mapNameText != null)
            mapNameText.text = name;
    }

    public void OnLogoutClick()
    {
        StartCoroutine(PlayerAPI.LogoutPlayer(
        onSuccess: () =>
        {
            Debug.Log("Đăng xuất thành công");
            if (WebSocketManager.Instance != null)
            {
                WebSocketManager.Instance.Disconnect();
                Destroy(WebSocketManager.Instance.gameObject);
                WebSocketManager.Instance = null;
                Debug.Log("🧹 Đã huỷ WebSocketManager");
            }
            
            SceneManager.LoadScene("LoginScene");
        },
        onError: (err) =>
        {
            Debug.LogError("Lỗi đăng xuất: " + err);
        }
        ));
    }
}
