using UnityEngine;

public class RemoteAuthSession : MonoBehaviour
{
    public static RemoteAuthSession instance { get; private set; }

    public string Username { get; private set; } = string.Empty;
    public string AccessToken { get; private set; } = string.Empty;

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSession(string username, string accessToken)
    {
        Username = username ?? string.Empty;
        AccessToken = accessToken ?? string.Empty;
    }

    public void Logout()
    {
        Username = string.Empty;
        AccessToken = string.Empty;
    }
}
