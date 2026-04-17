using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuContactsController : MonoBehaviour
{
    [SerializeField]
    private string contactEmail = "seuemail@exemplo.com";

    [SerializeField]
    private TMP_Text emailLabel;

    [SerializeField]
    private TMP_Text feedbackLabel;

    [SerializeField]
    private Button copyButton;

    private void Awake()
    {
        RefreshLabels();
    }

    private void OnEnable()
    {
        if (copyButton != null)
            copyButton.onClick.AddListener(CopyEmailToClipboard);
    }

    private void OnDisable()
    {
        if (copyButton != null)
            copyButton.onClick.RemoveListener(CopyEmailToClipboard);
    }

    public void CopyEmailToClipboard()
    {
        GUIUtility.systemCopyBuffer = contactEmail;
        SetFeedback("E-mail copiado para a área de transferência.");
    }

    private void RefreshLabels()
    {
        if (emailLabel != null)
            emailLabel.text = contactEmail;
    }

    private void SetFeedback(string message)
    {
        if (feedbackLabel != null)
            feedbackLabel.text = message;
    }
}
