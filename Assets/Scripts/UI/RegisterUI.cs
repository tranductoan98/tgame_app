using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput;
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
    public void OnRegisterButtonClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        string email = emailInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
        {
            errorText.text = "Vui lòng điền đầy đủ thông tin.";
            return;
        }

        StartCoroutine(UserAPI.Register(username, password, email,
            onSuccess: (res) =>
            {
                errorText.gameObject.SetActive(true);
                errorText.color = Color.green;
                errorText.text = "Đăng ký thành công!";

                SceneManager.LoadScene("LoginScene");
            },
            onError: (err) =>
            {
                errorText.gameObject.SetActive(true);
                errorText.color = Color.red;
                errorText.text = "Lỗi: " + err;
            }));
    }
}
