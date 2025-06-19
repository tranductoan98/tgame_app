using TMPro;
using UnityEngine;

public class MapSceneUI : MonoBehaviour
{
    public TextMeshProUGUI mapNameText;

    public void SetMapInfo(string name)
    {
         mapNameText.gameObject.SetActive(false);
        if (mapNameText != null)
            mapNameText.text = name;
    }
}
