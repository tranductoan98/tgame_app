using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void GoToRegisterScene()
    {
        SceneManager.LoadScene("RegisterScene");
    }

    public void GoToLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
