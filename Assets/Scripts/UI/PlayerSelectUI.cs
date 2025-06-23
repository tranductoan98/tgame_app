using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSlotUI
    {
        public Button slotButton;
        public TextMeshProUGUI playerId;
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI playerLevelText;
        public Image avatarImage;
    }

    public List<PlayerSlotUI> playerSlots;
    public GameObject registerPopup;
    public GameObject playerSelectPanel;
    public TMP_InputField playerNameInput;
    public TMP_Dropdown genderDropdown;
    public Button submitButton;
    public Button cancelButton;
    public TextMeshProUGUI errorText;
    public int userId;
    public string token;
    private List<PlayerData> playerDataList = new List<PlayerData>();
    private PlayerLoginManager loginManager;

    void Start()
    {
        userId = PlayerPrefs.GetInt("userId", -1);
        token = PlayerPrefs.GetString("token", "");

        registerPopup.SetActive(false);
        errorText.gameObject.SetActive(false);

        if (userId == -1 || string.IsNullOrEmpty(token))
        {
            Debug.LogError("Thiếu userId hoặc token. Trở về Login.");
            SceneManager.LoadScene("LoginScene");
            return;
        }

        loginManager = FindFirstObjectByType<PlayerLoginManager>();

        if (loginManager == null)
        {
            Debug.LogError("Không tìm thấy PlayerLoginManager trong scene!");
            return;
        }

        LoadPlayers();
    }

    void LoadPlayers()
    {
        StartCoroutine(PlayerAPI.GetPlayers(userId,
            onSuccess: (players) =>
            {
                playerDataList = players;
                
                for (int i = 0; i < playerSlots.Count; i++)
                {
                    if (i < playerDataList.Count)
                    {
                        var p = playerDataList[i];
                        playerSlots[i].playerId.text = "Id: " + p.playerid;
                        playerSlots[i].playerNameText.text = "Name: " + p.name;
                        playerSlots[i].playerLevelText.text = "Level: " + p.level;

                        PlayerSlot ps = playerSlots[i].slotButton.GetComponent<PlayerSlot>();
                        if (ps != null)
                        {
                            ps.Setup(p);
                        }
                        BindSlotButton(playerSlots[i].slotButton, true, p.playerid);

                    }
                    else
                    {
                        playerSlots[i].playerId.text = "";
                        playerSlots[i].playerNameText.text = "Chưa có nhân vật";
                        playerSlots[i].playerLevelText.text = "";
                        BindSlotButton(playerSlots[i].slotButton, false, i);
                    }
                }
            },
            onError: (err) =>
            {
                Debug.LogError("Lỗi load player: " + err);
            }));
    }

    public void OnBackToLogin()
    {
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.Save();

        Destroy(gameObject);
        SceneManager.LoadScene("LoginScene");
    }

    void OpenRegisterPopup()
    {
        playerNameInput.text = "";
        genderDropdown.value = 0;
        registerPopup.SetActive(true);
        playerSelectPanel.SetActive(false);
    }

    public void SubmitNewPlayer()
    {
        if (playerDataList.Count >= 3)
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Bạn chỉ được tạo tối đa 3 nhân vật.";
            return;
        }

        string name = playerNameInput.text;
        string gender = genderDropdown.options[genderDropdown.value].text;

        if (string.IsNullOrEmpty(name)) return;

        StartCoroutine(PlayerAPI.RegisterPlayer(userId, name, gender,
            onSuccess: (msg) =>
            {
                Debug.Log("Tạo thành công: " + msg);
                registerPopup.SetActive(false);
                playerSelectPanel.SetActive(true);
                LoadPlayers();
            },
            onError: (err) =>
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Tạo nhân vật thất bại: " + err;
            }));
    }

    public void CancelRegister()
    {
        registerPopup.SetActive(false);
        playerSelectPanel.SetActive(true);
    }

    void SelectPlayer(int index)
    {
        if (loginManager == null)
        {
            Debug.LogError("loginManager is null in SelectPlayer! Đảm bảo PlayerLoginManager đã được gán.");
            return;
        }

        loginManager.LoginPlayer(index);
    }

    void BindSlotButton(Button btn, bool hasPlayer, int playerIdOrSlotIndex)
    {
        btn.onClick.RemoveAllListeners();
        if (hasPlayer)
        {
            btn.onClick.AddListener(() => OnPlayerSlotClicked(true, playerIdOrSlotIndex));
        }
        else
        {
            btn.onClick.AddListener(() => OnPlayerSlotClicked(false, playerIdOrSlotIndex));
        }
    }

    
    public void OnPlayerSlotClicked(bool isPlayerId, int value)
    {
        if (isPlayerId)
        {
            SelectPlayer(value);
        }
        else
        {
            OpenRegisterPopup();
        }
    }
}