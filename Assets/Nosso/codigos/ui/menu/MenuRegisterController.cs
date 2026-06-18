using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuRegisterController : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField]
    private TMP_InputField nameInput;

    [SerializeField]
    private TMP_InputField usernameInput;

    [SerializeField]
    private TMP_InputField passwordInput;

    [Header("Feedback")]
    [SerializeField]
    private TMP_Text feedbackLabel;

    [Header("Button")]
    [SerializeField]
    private Button registerButton;

    [SerializeField]
    public RemoteSaveService remoteSaveService;

    private void Awake()
    {
        remoteSaveService = FindFirstObjectByType<RemoteSaveService>();

        if (remoteSaveService == null)
        {
            Debug.LogError("RemoteSaveService NÃO encontrado na cena");
        }
        else
        {
            Debug.Log("RemoteSaveService encontrado");
        }
    }

    private void OnEnable()
    {
        if (registerButton != null)
            registerButton.onClick.AddListener(Register);
    }

    private void OnDisable()
    {
        if (registerButton != null)
            registerButton.onClick.RemoveListener(Register);
    }

    public void Register()
    {
        string nome = nameInput.text.Trim();

        string login = usernameInput.text.Trim();

        string senha = passwordInput.text;

        if (
            string.IsNullOrEmpty(nome)
            || string.IsNullOrEmpty(login)
            || string.IsNullOrEmpty(senha)
        )
        {
            SetFeedback("Preencha todos os campos.");

            return;
        }

        if (remoteSaveService != null)
        {
            remoteSaveService.Register(login, senha);

            SetFeedback("Criando conta...");
        }
    }

    private void SetFeedback(string message)
    {
        if (feedbackLabel != null)
            feedbackLabel.text = message;
    }
}
