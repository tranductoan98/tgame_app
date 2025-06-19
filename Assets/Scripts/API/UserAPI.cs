using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json;

public class UserAPI : MonoBehaviour
{
    private static string baseUrl = $"{GameConfig.ApiBaseUrl}/user";
    public static IEnumerator Login(string username, string password, Action<LoginResponse> onSuccess, Action<string> onError)
    {
        var data = new { username, password };
        string json = JsonConvert.SerializeObject(data);

        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/login", "POST");
        byte[] body = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            try
            {
                var response = JsonConvert.DeserializeObject<LoginResponse>(responseText);
                onSuccess?.Invoke(response);
            }
            catch (Exception e)
            {
                onError?.Invoke("Lỗi parse response: " + e.Message);
            }
        }
    }

    public static IEnumerator Register(string username, string password, string email, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string json = JsonUtility.ToJson(new UserRegiterRequest { username = username, password = password, email = email });

        var request = new UnityWebRequest($"{baseUrl}/register", "POST");
        byte[] body = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

    #if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
    #else
        if (request.isNetworkError || request.isHttpError)
    #endif
        {
            onError?.Invoke(request.error);
        }
        else
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }

    [Serializable]
    private class UserLoginRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    private class UserRegiterRequest
    {
        public string username;
        public string password;
        public string email;

    }
}
