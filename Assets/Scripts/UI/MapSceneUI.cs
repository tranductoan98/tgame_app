using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSceneUI : MonoBehaviour
{
    [Header("Setting UI")]
    public Button BtnSetting;
    public Button BtnExitSetting;
    public GameObject Panel_Setting;

    [Header("Chat UI")]
    public GameObject Panel_ChatArea;
    public GameObject Panel_ChatAreaAll;
    public Button btnGlobal;
    public Button btnMap;
    public Button btnPrivate;
    public Button BtnSendChat;
    public Button BtnExitChatArea;
    public TMP_InputField txtInputChat;
    public Transform chatContent;
    public GameObject chatMessagePrefab;
    private string currentChannel = "global";
    private List<GameObject> chatItems = new List<GameObject>();
    void Start()
    {
        // Setting
        Panel_Setting.SetActive(false);
        Panel_ChatArea.SetActive(false);
        BtnSetting.onClick.AddListener(OnSettingClick);

        // Chat buttons
        btnGlobal.onClick.AddListener(() => SwitchChannel("global"));
        btnMap.onClick.AddListener(() => SwitchChannel("map"));
        btnPrivate.onClick.AddListener(() => SwitchChannel("private"));
        BtnSendChat.onClick.AddListener(OnSendChatClick);

        txtInputChat.onSelect.AddListener(OnFocus);
        txtInputChat.onDeselect.AddListener(OnUnfocus);
    }
    // Setting
    public void OnSettingClick()
    {
        Panel_Setting.SetActive(true);
    }
    public void OnExitSettingClick()
    {
        Panel_Setting.SetActive(false);
    }
    public void OnLogoutClick()
    {
        if (MapSceneManager.Instance != null)
            MapSceneManager.Instance.HandleLogout();
        else
            Debug.LogError("❌ Không tìm thấy MapSceneManager.Instance!");
    }
    // ============ CHAT METHODS ============
    public void OnScrollView_ChatAllClick()
    {
        Panel_ChatArea.SetActive(true);
        Panel_ChatAreaAll.SetActive(false);
    }
    public void OnExitChatAreaClick()
    {
        Panel_ChatArea.SetActive(false);
        Panel_ChatAreaAll.SetActive(true);
    }
    void OnFocus(string text)
    {
        GameInputConfig.Instance?.SetInputFieldFocused(true);
    }
    void OnUnfocus(string text)
    {
        GameInputConfig.Instance?.SetInputFieldFocused(false);
    }

    public void AddChatMessage(string sender, string message, string channel)
    {
        GameObject chatObj = Instantiate(chatMessagePrefab, chatContent);
        chatObj.GetComponent<TMP_Text>().text = $"[{channel}] {sender}: {message}";
        chatObj.tag = channel;

        chatItems.Add(chatObj);
        if (chatItems.Count > 100)
        {
            Destroy(chatItems[0]);
            chatItems.RemoveAt(0);
        }
        RefreshChatDisplay();
    }

    void SwitchChannel(string channel)
    {
        currentChannel = channel;
        RefreshChatDisplay();

        btnGlobal.image.color = (channel == "global") ? Color.yellow : Color.white;
        btnMap.image.color = (channel == "map") ? Color.yellow : Color.white;
        btnPrivate.image.color = (channel == "private") ? Color.yellow : Color.white;
    }

    void RefreshChatDisplay()
    {
        foreach (var item in chatItems)
        {
            item.SetActive(item.tag == currentChannel);
        }

        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = chatContent.GetComponentInParent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    void OnSendChatClick()
    {
        string message = txtInputChat.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log($"📤 Gửi tin nhắn [{currentChannel}]: {message}");

            AddChatMessage("Me", message, currentChannel);
            txtInputChat.text = "";
        }
    }
}