using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseSaveButton : MonoBehaviour
{
    [SerializeField] private Button saveButton;
    [SerializeField] private TMP_Text feedbackLabel; // opcional

    private void OnEnable()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);
    }

    private void OnDisable()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveListener(OnSaveClicked);
    }

    private void OnSaveClicked()
    {
        var service = RemoteSaveService.getInstance();

        if (service == null)
        {
            Debug.LogError("RemoteSaveService não encontrado!");
            return;
        }

        service.SaveGame();

        if (feedbackLabel != null)
            feedbackLabel.text = "Jogo salvo!";

        Debug.Log("Save solicitado pelo pause.");
    }
}