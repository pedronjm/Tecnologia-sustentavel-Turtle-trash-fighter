using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuLoginController : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField]
    private TMP_InputField usernameInput;

    [SerializeField]
    private TMP_InputField passwordInput;

    [Header("Feedback")]
    [SerializeField]
    private TMP_Text feedbackLabel;

    [Header("Buttons")]
    [SerializeField]
    private Button loginButton;

    [SerializeField]
    private RemoteSaveService remoteSaveService;

    private void Awake()
    {
        remoteSaveService = FindFirstObjectByType<RemoteSaveService>();
    }

    private void OnEnable()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(Login);
    }

    private void OnDisable()
    {
        if (loginButton != null)
            loginButton.onClick.RemoveListener(Login);
    }

    public void Login()
    {
        Debug.Log("BOTAO CLICADO");

        if (usernameInput == null || passwordInput == null)
        {
            Debug.LogError("Input não encontrado");
            return;
        }

        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        Debug.Log("Usuario: " + username);

        remoteSaveService.Login(username, password);
    }

    private void SetFeedback(string message)
    {
        if (feedbackLabel != null)
            feedbackLabel.text = message;
    }
}
