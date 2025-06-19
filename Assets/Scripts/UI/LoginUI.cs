using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    public Toggle showPasswordToggle;

    void Start()
    {
        errorText.gameObject.SetActive(false);
        if (showPasswordToggle != null)
        {
            showPasswordToggle.onValueChanged.AddListener(OnToggleShowPassword);
        }
    }
    
    public void OnToggleShowPassword(bool isOn)
    {
        if (isOn)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
        }
        passwordInput.ForceLabelUpdate();
    }

    public void OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Vui lòng nhập đầy đủ thông tin";
            return;
        }

        StartCoroutine(UserAPI.Login(username, password,
            onSuccess: response =>
            {
                PlayerPrefs.SetString("token", response.token);
                PlayerPrefs.SetInt("userId", response.user.id);
                PlayerPrefs.Save();
                errorText.gameObject.SetActive(false);
                SceneManager.LoadScene("PlayerSelectScene");
            },
            onError: err =>
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Sai tài khoản hoặc mật khẩu";
            }
        ));
    }
}
